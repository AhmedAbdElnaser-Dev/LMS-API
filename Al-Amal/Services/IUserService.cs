using Al_Amal.DTOs;
using Al_Amal.Models;
using Microsoft.AspNetCore.Identity;

namespace Al_Amal.Services;

public interface IUserService
{
    Task<(IdentityResult Result, ApplicationUser? User)> RegisterUserAsync(UserRegisterDTO userDto);
    Task<(ApplicationUser? User, string? Token)> AuthenticateUserAsync(UserLoginDTO loginDto);
    Task<ApplicationUser?> GetUserByIdAsync(Guid id);
    Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
    Task<IList<string>> GetUserPermissionsAsync(ApplicationUser user);
}