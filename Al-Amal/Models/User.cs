using System.ComponentModel.DataAnnotations;

namespace Al_Amal.Models;

public class User
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required.")]
    [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be 'Male', 'Female', or 'Other'.")]
    public string Gender { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Telegram username cannot exceed 100 characters.")]
    public string? TelegramNumber { get; set; }

    [StringLength(50, ErrorMessage = "Time zone cannot exceed 50 characters.")]
    public string Timezone { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Country name cannot exceed 50 characters.")]
    public string Country { get; set; } = string.Empty;

    public DateTime? AvailableTime { get; set; }
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
