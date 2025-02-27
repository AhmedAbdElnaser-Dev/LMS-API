namespace LMS_API.Controllers.Users.ViewModels
{
    public class VerifyUserVM
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Telegram { get; set; }
        public string Country { get; set; }
        public string TimeZone { get; set; }
        public string Role { get; set; }
    }
}
