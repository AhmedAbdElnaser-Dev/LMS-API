using Al_Amal.Data;
using Al_Amal.DTOs;
using Al_Amal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Al_Amal.Services;

public class UserService : IUserService
{
    private readonly ApplicationDBContext _dbContext;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;

    public UserService(ApplicationDBContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = new PasswordHasher<User>();
        _tokenService = tokenService;
    }

    public async Task<bool> RegisterUserAsync(UserRegisterDTO userDto)
    {
        if (await _dbContext.Users.AnyAsync(u =>
            u.Email == userDto.Email ||
            u.TelegramNumber == userDto.Telegram ||
            u.PhoneNumber == userDto.Phone))
        {
            return false;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = userDto.Name,
            Age = userDto.Age,
            Email = userDto.Email,
            Gender = userDto.Gender,
            PhoneNumber = userDto.Phone,
            TelegramNumber = userDto.Telegram,
            Timezone = userDto.TimeZone,
            Country = userDto.Country,
            RoleId = await GetDefaultRoleId()
        };

        user.Password = _passwordHasher.HashPassword(user, userDto.Password);

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<(User? User, string? Token)> AuthenticateUserAsync(UserLoginDTO loginDto)
    {
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null)
            return (null, null);

        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
        if (result != PasswordVerificationResult.Success)
            return (null, null);

        var permissions = await GetUserPermissionsAsync(user.Id);
        var token = _tokenService.GenerateJwtToken(user, permissions);

        return (user, token);
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IList<string>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.Permissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return new List<string>();

        return user.Role.Permissions
            .Select(rp => rp.Permission.SystemName)
            .ToList();
    }

    private async Task<Guid> GetDefaultRoleId()
    {
        var studentRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
        if (studentRole == null)
        {
            studentRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Student",
                Description = "Default role for new users"
            };
            await _dbContext.Roles.AddAsync(studentRole);
            await _dbContext.SaveChangesAsync();
        }
        return studentRole.Id;
    }
}