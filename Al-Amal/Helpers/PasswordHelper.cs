using Microsoft.AspNetCore.Identity;

namespace Al_Amal.Helpers
{
    public class PasswordHelper
    {
        private readonly PasswordHasher<object> _passwordHasher;

        public PasswordHelper()
        {
            _passwordHasher = new PasswordHasher<object>();
        }

        public string HashPassword(string plainTextPassword)
        {
            return _passwordHasher.HashPassword(null, plainTextPassword);
        }

        public bool VerifyPassword(string plainTextPassword, string hashedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, plainTextPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
