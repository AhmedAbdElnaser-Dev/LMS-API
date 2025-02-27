namespace Al_Amal.DTOs;

public class UserResponseDTO
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? TelegramNumber { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}