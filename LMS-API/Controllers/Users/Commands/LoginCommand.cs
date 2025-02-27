namespace LMS_API.Controllers.Users.Commands
{
    public class LoginCommand
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
