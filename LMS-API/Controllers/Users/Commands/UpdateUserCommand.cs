namespace LMS_API.Controllers.Users.Commands
{
    public class UpdateUserCommand
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }
}
