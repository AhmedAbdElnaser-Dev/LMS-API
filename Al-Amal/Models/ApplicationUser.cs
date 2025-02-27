using Microsoft.AspNetCore.Identity;

namespace Al_Amal.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? TelegramNumber { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}