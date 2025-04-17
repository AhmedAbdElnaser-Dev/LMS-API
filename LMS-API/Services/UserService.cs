using AutoMapper;
using LMS_API.Controllers.Courses.ViewModels;
using LMS_API.Controllers.Users.Commands;
using LMS_API.Controllers.Users.ViewModels;
using LMS_API.Data;
using LMS_API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS_API.Services
{
    public interface IUserService
    {
        Task<(bool Success, UserVM? User, IEnumerable<IdentityError>? Errors)> RegisterUser(RegisterUserCommand command);
        Task<(bool Success, string Message)> LoginUserAsync(LoginCommand model);
        Task<VerifyUserVM?> VerifyUserByIdAsync(string userId);
        Task<List<UserVM>> GetAllUsersAsync();
        Task<UserVM> GetUserByIdAsync(string userId);
        Task<VerifyUserVM> AddUserAsync(string currentUserId, AddUserCommand model);
        Task<VerifyUserVM> UpdateUserAsync(string currentUserId, string userId, UpdateUserCommand command);
        Task DeleteUserAsync(string currentUserId, string userId);
        Task<List<UserViewModel>> GetStudentsAsync();
        Task<List<UserViewModel>> GetTeachersAsync();
    }

    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly DBContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            DBContext context,
            ILogger<UserService> logger,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<(bool Success, UserVM? User, IEnumerable<IdentityError>? Errors)> RegisterUser(RegisterUserCommand command)
        {
            var user = _mapper.Map<ApplicationUser>(command);
            var result = await _userManager.CreateAsync(user, command.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to register user {Email}: {Errors}", command.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return (false, null, result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "Student");

            var userVM = _mapper.Map<UserVM>(user);
            _logger.LogInformation("User {Email} registered successfully", command.Email);
            return (true, userVM, null);
        }

        public async Task<(bool Success, string Message)> LoginUserAsync(LoginCommand model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                _logger.LogWarning("Invalid login attempt for {Email}", model.Email);
                return (false, "Invalid credentials");
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                _logger.LogWarning("Failed login attempt for {Email}", model.Email);
                return (false, "Invalid login attempt");
            }

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(5) : DateTimeOffset.UtcNow.AddHours(24)
            };

            await _signInManager.SignInAsync(user, authProperties);
            _logger.LogInformation("User {Email} logged in successfully", model.Email);
            return (true, "Login successful");
        }

        public async Task<VerifyUserVM?> VerifyUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for verification", userId);
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("Verified user {UserId}", userId);
            return new VerifyUserVM
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Telegram = user.TelegramNumber,
                Country = user.Country,
                TimeZone = user.Timezone,
                Role = roles.FirstOrDefault() ?? "No Role"
            };
        }

        public async Task<List<UserVM>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserVM
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    TelegramNumber = user.TelegramNumber,
                    Country = user.Country,
                    Age = user.Age,
                    Timezone = user.Timezone,
                    Role = roles.FirstOrDefault() ?? "No Role"
                });
            }

            _logger.LogInformation("Retrieved {Count} users", userList.Count);
            return userList;
        }

        public async Task<UserVM> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userVM = new UserVM
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                TelegramNumber = user.TelegramNumber,
                Country = user.Country,
                Age = user.Age,
                Timezone = user.Timezone,
                Role = roles.FirstOrDefault() ?? "No Role"
            };

            _logger.LogInformation("Retrieved user {UserId}", userId);
            return userVM;
        }

        public async Task<VerifyUserVM> AddUserAsync(string currentUserId, AddUserCommand model)
        {
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
            {
                _logger.LogWarning("Current user {UserId} not found", currentUserId);
                throw new UnauthorizedAccessException("Current user not found");
            }

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin");
            bool isAdmin = currentUserRoles.Contains("Admin");
            bool isManager = currentUserRoles.Contains("Manager");

            if (!isSuperAdmin && !isAdmin && !isManager)
            {
                _logger.LogWarning("User {UserId} lacks permission to add users", currentUserId);
                throw new UnauthorizedAccessException("You do not have permission to add users");
            }

            if (isAdmin && model.Role == "SuperAdmin")
            {
                _logger.LogWarning("Admin {UserId} attempted to add SuperAdmin", currentUserId);
                throw new UnauthorizedAccessException("Admin cannot add SuperAdmin");
            }

            if (isManager && (model.Role == "SuperAdmin" || model.Role == "Admin"))
            {
                _logger.LogWarning("Manager {UserId} attempted to add {Role}", currentUserId, model.Role);
                throw new UnauthorizedAccessException("Manager cannot add SuperAdmin or Admin");
            }

            var newUser = _mapper.Map<ApplicationUser>(model);
            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to create user {Email}: {Errors}", model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await _userManager.AddToRoleAsync(newUser, model.Role);

            _logger.LogInformation("User {Email} added by {CurrentUserId}", model.Email, currentUserId);
            return new VerifyUserVM
            {
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Role = model.Role
            };
        }

        public async Task<VerifyUserVM> UpdateUserAsync(string currentUserId, string userId, UpdateUserCommand command)
        {
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
            {
                _logger.LogWarning("Current user {UserId} not found", currentUserId);
                throw new UnauthorizedAccessException("Current user not found");
            }

            var userToUpdate = await _userManager.FindByIdAsync(userId);
            if (userToUpdate == null)
            {
                _logger.LogWarning("User to update {UserId} not found", userId);
                throw new Exception("User to update not found");
            }

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin");
            bool isAdmin = currentUserRoles.Contains("Admin");

            if (!isSuperAdmin && !isAdmin)
            {
                _logger.LogWarning("User {UserId} lacks permission to update users", currentUserId);
                throw new UnauthorizedAccessException("You do not have permission to update users");
            }

            if (isAdmin && command.Role == "SuperAdmin")
            {
                _logger.LogWarning("Admin {UserId} attempted to assign SuperAdmin role", currentUserId);
                throw new UnauthorizedAccessException("Admin cannot assign SuperAdmin role");
            }

            if (!string.IsNullOrWhiteSpace(command.Email))
                userToUpdate.Email = command.Email;

            if (!string.IsNullOrWhiteSpace(command.FirstName))
                userToUpdate.FirstName = command.FirstName;

            if (!string.IsNullOrWhiteSpace(command.LastName))
                userToUpdate.LastName = command.LastName;

            var updateResult = await _userManager.UpdateAsync(userToUpdate);
            if (!updateResult.Succeeded)
            {
                _logger.LogWarning("Failed to update user {UserId}: {Errors}", userId, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                throw new Exception($"Failed to update user: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
            }

            if (!string.IsNullOrWhiteSpace(command.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(userToUpdate);
                await _userManager.RemoveFromRolesAsync(userToUpdate, currentRoles);
                var roleResult = await _userManager.AddToRoleAsync(userToUpdate, command.Role);
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to update role for user {UserId}: {Errors}", userId, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    throw new Exception($"Failed to update role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }

            if (!string.IsNullOrWhiteSpace(command.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(userToUpdate);
                var passwordResult = await _userManager.ResetPasswordAsync(userToUpdate, token, command.Password);
                if (!passwordResult.Succeeded)
                {
                    _logger.LogWarning("Failed to reset password for user {UserId}: {Errors}", userId, string.Join(", ", passwordResult.Errors.Select(e => e.Description)));
                    throw new Exception($"Failed to reset password: {string.Join(", ", passwordResult.Errors.Select(e => e.Description))}");
                }
            }

            _logger.LogInformation("User {UserId} updated by {CurrentUserId}", userId, currentUserId);
            return new VerifyUserVM
            {
                Email = userToUpdate.Email,
                FirstName = userToUpdate.FirstName,
                LastName = userToUpdate.LastName,
                Role = command.Role ?? (await _userManager.GetRolesAsync(userToUpdate)).FirstOrDefault()
            };
        }

        public async Task DeleteUserAsync(string currentUserId, string userId)
        {
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
            {
                _logger.LogWarning("Current user {UserId} not found", currentUserId);
                throw new UnauthorizedAccessException("Current user not found");
            }

            var userToDelete = await _userManager.FindByIdAsync(userId);
            if (userToDelete == null)
            {
                _logger.LogWarning("User to delete {UserId} not found", userId);
                throw new Exception("User to delete not found");
            }

            if (currentUserId == userId)
            {
                _logger.LogWarning("User {UserId} attempted to delete themselves", currentUserId);
                throw new UnauthorizedAccessException("You cannot delete yourself");
            }

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin");
            bool isAdmin = currentUserRoles.Contains("Admin");

            if (!isSuperAdmin && !isAdmin)
            {
                _logger.LogWarning("User {UserId} lacks permission to delete users", currentUserId);
                throw new UnauthorizedAccessException("You do not have permission to delete users");
            }

            var deleteResult = await _userManager.DeleteAsync(userToDelete);
            if (!deleteResult.Succeeded)
            {
                _logger.LogWarning("Failed to delete user {UserId}: {Errors}", userId, string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
                throw new Exception($"Failed to delete user: {string.Join(", ", deleteResult.Errors.Select(e => e.Description))}");
            }

            _logger.LogInformation("User {UserId} deleted by {CurrentUserId}", userId, currentUserId);
        }

        public async Task<List<UserViewModel>> GetStudentsAsync()
        {
            try
            {
                var studentRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "Student");
                if (studentRole == null)
                {
                    _logger.LogWarning("Student role not found");
                    return new List<UserViewModel>();
                }

                var students = await _context.Users
                    .Join(_context.UserRoles,
                        user => user.Id,
                        userRole => userRole.UserId,
                        (user, userRole) => new { User = user, UserRole = userRole })
                    .Where(ur => ur.UserRole.RoleId == studentRole.Id)
                    .Select(ur => new UserViewModel
                    {
                        Id = ur.User.Id,
                        Email = ur.User.Email,
                        FullName = $"{ur.User.FirstName} {ur.User.LastName}".Trim(),
                    })
                    .ToListAsync();

                return students;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students");
                throw new Exception("Failed to retrieve students", ex);
            }
        }

        public async Task<List<UserViewModel>> GetTeachersAsync()
        {
            try
            {
                var teacherRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "Teacher");
                if (teacherRole == null)
                {
                    _logger.LogWarning("Teacher role not found");
                    return new List<UserViewModel>();
                }

                var teachers = await _context.Users
                    .Join(_context.UserRoles,
                        user => user.Id,
                        userRole => userRole.UserId,
                        (user, userRole) => new { User = user, UserRole = userRole })
                    .Where(ur => ur.UserRole.RoleId == teacherRole.Id)
                    .Select(ur => new UserViewModel
                    {
                        Id = ur.User.Id,
                        Email = ur.User.Email,
                        FullName = $"{ur.User.FirstName} {ur.User.LastName}".Trim(),
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} teachers", teachers.Count);
                return teachers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teachers");
                throw new Exception("Failed to retrieve teachers", ex);
            }
        }
    }
}
