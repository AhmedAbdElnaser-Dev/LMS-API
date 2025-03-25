using AutoMapper;
using LMS_API.Controllers.Users.Commands;
using LMS_API.Controllers.Users.ViewModels;
using LMS_API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS_API.Services
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, ILogger<UserService> logger, IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<(bool Success, UserVM? User, IEnumerable<IdentityError>? Errors)> RegisterUser(RegisterUserCommand command)
        {
            var user = _mapper.Map<ApplicationUser>(command);
            var result = await _userManager.CreateAsync(user, command.Password);

            if (!result.Succeeded)
                return (false, null, result.Errors);

            await _userManager.AddToRoleAsync(user, "Student");

            var userVM = _mapper.Map<UserVM>(user);
            return (true, userVM, null);
        }

        public async Task<(bool Success, string Message)> LoginUserAsync(LoginCommand model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return (false, "Invalid credentials");

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
                return (false, "Invalid login attempt");

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(5) : DateTimeOffset.UtcNow.AddHours(24)
            };

            await _signInManager.SignInAsync(user, authProperties);
            _logger.LogInformation("User {Email} logged in successfully.", model.Email);

            return (true, "Login successful");
        }

        public async Task<VerifyUserVM?> VerifyUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
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
            var users = _userManager.Users.ToList();
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
            return userList;
        }

        public async Task<UserVM> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new UserVM
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
        }

        public async Task<VerifyUserVM> AddUserAsync(string currentUserId, AddUserCommand model)
        {
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin");
            bool isAdmin = currentUserRoles.Contains("Admin");
            bool isManager = currentUserRoles.Contains("Manager");

            if (!isSuperAdmin && !isAdmin && !isManager)
                throw new UnauthorizedAccessException("You do not have permission to add users");

            if (isAdmin && model.Role == "SuperAdmin")
                throw new UnauthorizedAccessException("Admin cannot add SuperAdmin");

            if (isManager && (model.Role == "SuperAdmin" || model.Role == "Admin"))
                throw new UnauthorizedAccessException("Manager cannot add SuperAdmin or Admin");

            var newUser = _mapper.Map<ApplicationUser>(model);
            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (!result.Succeeded)
                throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await _userManager.AddToRoleAsync(newUser, model.Role);

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
                throw new UnauthorizedAccessException("Current user not found");

            var userToUpdate = await _userManager.FindByIdAsync(userId);
            if (userToUpdate == null)
                throw new Exception("User to update not found");

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin");
            bool isAdmin = currentUserRoles.Contains("Admin");

            if (!isSuperAdmin && !isAdmin)
                throw new UnauthorizedAccessException("You do not have permission to update users");

            if (isAdmin && command.Role == "SuperAdmin")
                throw new UnauthorizedAccessException("Admin cannot assign SuperAdmin role");

            if (!string.IsNullOrWhiteSpace(command.Email))
                userToUpdate.Email = command.Email;

            if (!string.IsNullOrWhiteSpace(command.FirstName))
                userToUpdate.FirstName = command.FirstName;

            if (!string.IsNullOrWhiteSpace(command.LastName))
                userToUpdate.LastName = command.LastName;

            var updateResult = await _userManager.UpdateAsync(userToUpdate);
            if (!updateResult.Succeeded)
                throw new Exception($"Failed to update user: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");

            if (!string.IsNullOrWhiteSpace(command.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(userToUpdate);
                await _userManager.RemoveFromRolesAsync(userToUpdate, currentRoles);
                var roleResult = await _userManager.AddToRoleAsync(userToUpdate, command.Role);
                if (!roleResult.Succeeded)
                    throw new Exception($"Failed to update role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }

            if (!string.IsNullOrWhiteSpace(command.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(userToUpdate);
                var passwordResult = await _userManager.ResetPasswordAsync(userToUpdate, token, command.Password);
                if (!passwordResult.Succeeded)
                    throw new Exception($"Failed to reset password: {string.Join(", ", passwordResult.Errors.Select(e => e.Description))}");
            }

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
                throw new UnauthorizedAccessException("Current user not found");

            var userToDelete = await _userManager.FindByIdAsync(userId);
            if (userToDelete == null)
                throw new Exception("User to delete not found");

            if (currentUserId == userId)
                throw new UnauthorizedAccessException("You cannot delete yourself");

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin");
            bool isAdmin = currentUserRoles.Contains("Admin");

            if (!isSuperAdmin && !isAdmin)
                throw new UnauthorizedAccessException("You do not have permission to delete users");

            var deleteResult = await _userManager.DeleteAsync(userToDelete);
            if (!deleteResult.Succeeded)
                throw new Exception($"Failed to delete user: {string.Join(", ", deleteResult.Errors.Select(e => e.Description))}");
        }
    }
}
