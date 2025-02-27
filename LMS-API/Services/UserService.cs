using AutoMapper;
using LMS_API.Controllers.Users.Commands;
using LMS_API.Controllers.Users.ViewModels;
using LMS_API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
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
            {
                return (false, "Invalid credentials");
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                return (false, "Invalid login attempt");
            }

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


        public async Task<VerifyUserVM> AddUserAsync(string currentUserId, AddUserCommand model)
        {
            // Get current user
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                throw new UnauthorizedAccessException("User not found");

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            // Define role hierarchy rules
            bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin");
            bool isAdmin = currentUserRoles.Contains("Admin");
            bool isManager = currentUserRoles.Contains("Manager");

            if (isSuperAdmin) { /* SuperAdmin can add anyone */ }
            else if (isAdmin && (model.Role == "SuperAdmin"))
                throw new UnauthorizedAccessException("Admin cannot add SuperAdmin");
            else if (isManager && (model.Role == "SuperAdmin" || model.Role == "Admin"))
                throw new UnauthorizedAccessException("Manager cannot add SuperAdmin or Admin");
            else if (!isSuperAdmin && !isAdmin && !isManager)
                throw new UnauthorizedAccessException("You do not have permission to add users");

            // Create new user
            var newUser = _mapper.Map<ApplicationUser>(model);

            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Assign role
            await _userManager.AddToRoleAsync(newUser, model.Role);

            return new VerifyUserVM
            {
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Role = model.Role
            };
        }
    }
}
