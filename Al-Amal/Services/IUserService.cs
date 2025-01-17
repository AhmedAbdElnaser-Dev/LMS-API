using Al_Amal.DTOs;
using Al_Amal.Models;

namespace Al_Amal.Services;

public interface IUserService
{
    Task<bool> RegisterUserAsync(UserRegisterDTO userDto);
    Task<(User? User, string? Token)> AuthenticateUserAsync(UserLoginDTO loginDto);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<IList<string>> GetUserPermissionsAsync(Guid userId);
}