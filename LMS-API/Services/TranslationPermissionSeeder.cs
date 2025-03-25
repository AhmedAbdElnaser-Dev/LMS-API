using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS_API.Services
{
    public class TranslationPermissionSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<TranslationPermissionSeeder> _logger;

        public TranslationPermissionSeeder(
            RoleManager<IdentityRole> roleManager,
            ILogger<TranslationPermissionSeeder> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            // Define roles that will have translation permissions
            var roles = new[] { "SuperAdmin", "Admin", "Manager", "Teacher", "Student" };

            // Define translation permissions
            var translationPermissions = new[] { "Translate_ar", "Translate_en", "Translate_ru" };

            // Define which roles get which translation permissions initially
            var rolePermissions = new Dictionary<string, string[]>
            {
                { "SuperAdmin", translationPermissions },
                { "Admin", translationPermissions },    
                { "Manager", new[] { "Translate_en", "Translate_ar" } }, 
            };

            // Seed roles and assign permissions
            foreach (var roleName in roles)
            {
                // Create role if it doesn't exist
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName);
                    var result = await _roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        _logger.LogError("Failed to create role {RoleName}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                        continue;
                    }
                    _logger.LogInformation("Created role {RoleName}", roleName);
                }

                // Assign translation permissions to the role
                var roleEntity = await _roleManager.FindByNameAsync(roleName);
                if (roleEntity != null && rolePermissions.ContainsKey(roleName))
                {
                    var existingClaims = await _roleManager.GetClaimsAsync(roleEntity);
                    foreach (var permission in rolePermissions[roleName])
                    {
                        if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                        {
                            var claim = new Claim("Permission", permission);
                            var result = await _roleManager.AddClaimAsync(roleEntity, claim);
                            if (result.Succeeded)
                            {
                                _logger.LogInformation("Added permission {Permission} to role {Role}", permission, roleName);
                            }
                            else
                            {
                                _logger.LogError("Failed to add permission {Permission} to role {Role}: {Errors}", permission, roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                            }
                        }
                    }
                }
            }
        }
    }
}