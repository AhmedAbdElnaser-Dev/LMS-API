using Al_Amal.DTOs;
using Al_Amal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Al_Amal.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<(IdentityResult Result, ApplicationUser? User)> RegisterUserAsync(UserRegisterDTO userDto)
    {
        var user = new ApplicationUser
        {
            UserName = userDto.Email,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Age = userDto.Age,
            Gender = userDto.Gender,
            PhoneNumber = userDto.Phone,
            TelegramNumber = userDto.Telegram,
            Timezone = userDto.TimeZone,
            Country = userDto.Country
        };

        var result = await _userManager.CreateAsync(user, userDto.Password);
        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync("Student"))
            {
                await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = "Student",
                    Description = "Default role for new users"
                });
            }
            await _userManager.AddToRoleAsync(user, "Student");
            return (result, user);
        }

        return (result, null);
    }

    public async Task<(ApplicationUser? User, string? Token)> AuthenticateUserAsync(UserLoginDTO loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
            return (null, null);

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
            return (null, null);

        var roles = await GetUserRolesAsync(user);
        var permissions = await GetUserPermissionsAsync(user);
        var token = _tokenService.GenerateJwtToken(user, roles, permissions);

        return (user, token);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(Guid id)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IList<string>> GetUserPermissionsAsync(ApplicationUser user)
    {
        var roles = await GetUserRolesAsync(user);
        var permissions = new List<string>();

        // Here you would implement your permission logic based on roles
        // This is just a placeholder - implement according to your needs
        foreach (var role in roles)
        {
            // Add permissions based on role
            if (role == "Admin")
            {
                permissions.AddRange(new[] { "create", "read", "update", "delete" });
            }
            else if (role == "Student")
            {
                permissions.AddRange(new[] { "read" });
            }
        }

        return permissions.Distinct().ToList();
    }
}