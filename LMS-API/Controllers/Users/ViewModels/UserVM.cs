namespace LMS_API.Controllers.Users.ViewModels
{
    public class UserVM
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Timezone { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string TelegramNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
