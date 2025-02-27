using Microsoft.AspNetCore.Identity;

namespace Al_Amal.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public string Description { get; set; } = string.Empty;
    public virtual ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
}