using Al_Amal.Data;
using Al_Amal.DTOs;
using Al_Amal.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Al_Amal.Services
{
    public class UserService
    {
        private readonly ApplicationDBContext _dbContext;

        public UserService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> RegisterUserAsync(UserRegisterDTO userDto)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == userDto.Email || u.Telegram == userDto.Telegram || u.Phone == u.Phone))
                return false; 

            var hashedPassword = HashPassword(userDto.Password);

            var user = new User
            {
                Name = userDto.Name,
                Age = userDto.Age,
                Email = userDto.Email,
                Password = hashedPassword,
                Gender = userDto.Gender,
                Phone = userDto.Phone,
                Telegram = userDto.Telegram,
                TimeZone = userDto.TimeZone,
                Country = userDto.Country
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<User?> AuthenticateUserAsync(UserLoginDTO loginDto)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null || !VerifyPassword(loginDto.Password, user.Password))
                return null;

            return user;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string inputPassword, string storedHashedPassword)
        {
            var hashedInput = HashPassword(inputPassword);
            return hashedInput == storedHashedPassword;
        }
    }
}
