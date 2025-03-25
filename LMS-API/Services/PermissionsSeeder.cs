using LMS_API.Data;
using LMS_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LMS_API.Services
{
    public class PermissionsSeeder
    {
        private readonly DBContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<PermissionsSeeder> _logger;

        public PermissionsSeeder(DBContext context, RoleManager<IdentityRole> roleManager, ILogger<PermissionsSeeder> logger)
        {
            _context = context;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                _context.Database.SetCommandTimeout(60);

                var roles = new[] { "SuperAdmin", "Admin" };

                foreach (var roleName in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                var languages = new[] { "ar", "en", "ru" };

                var courseBasePermissions = new[]
                {
                    "Add_Course",
                    "Update_Course_Category",
                    "Update_Course_Books",
                    "Remove_Course_Book",
                    "View_Course",
                    "View_Courses",
                    "Add_Unit",
                    "View_Unit",
                    "View_Units",
                    "Update_Unit",
                    "Delete_Unit",
                    "Add_Group",
                    "View_Group",
                    "View_Groups",
                    "Update_Group",
                    "Delete_Group"
                };

                var bookBasePermissions = new[]
                {
                    "View_Books",
                    "View_Book",
                    "View_Book_Translations",
                    "Add_Book",
                    "Add_Book_Translation",
                    "Delete_Book",
                    "Delete_Book_Translation",
                    "Update_Book_Picture",
                    "Update_Book_Pdf",
                    "Update_Book_Translation"
                };

                var basePermissions = courseBasePermissions.Concat(bookBasePermissions).ToArray();

                var globalTranslatePermissions = languages.Select(lang => $"Translate_{lang}").ToArray();

                var specificTranslatePermissions = new List<string>();
                foreach (var permission in basePermissions)
                {
                    foreach (var lang in languages)
                    {
                        specificTranslatePermissions.Add($"{permission}_Translate_{lang}");
                    }
                }

                var allPermissions = basePermissions
                    .Concat(globalTranslatePermissions)
                    .Concat(specificTranslatePermissions)
                    .ToArray();

                var existingPermissions = await _context.Permissions
                    .Where(p => allPermissions.Contains(p.Name))
                    .Select(p => p.Name)
                    .ToListAsync();

                var permissionsToAdd = allPermissions
                    .Where(p => !existingPermissions.Contains(p))
                    .Select(p => new Permission { Name = p })
                    .ToList();

                if (permissionsToAdd.Any())
                {
                    _context.Permissions.AddRange(permissionsToAdd);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Added {Count} new permissions.", permissionsToAdd.Count);
                }

                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        var permissions = await _context.Permissions
                            .Where(p => allPermissions.Contains(p.Name))
                            .ToDictionaryAsync(p => p.Name, p => p.Id);

                        var existingRolePermissions = await _context.RolePermissions
                            .Where(rp => rp.RoleId == role.Id)
                            .Select(rp => rp.PermissionId)
                            .ToListAsync();

                        var rolePermissionsToAdd = permissions
                            .Where(p => allPermissions.Contains(p.Key) && !existingRolePermissions.Contains(p.Value))
                            .Select(p => new RolePermission
                            {
                                RoleId = role.Id,
                                PermissionId = p.Value
                            })
                            .ToList();

                        if (rolePermissionsToAdd.Any())
                        {
                            _context.RolePermissions.AddRange(rolePermissionsToAdd);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("Assigned {Count} permissions to role {Role}.", rolePermissionsToAdd.Count, roleName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed permissions.");
                throw;
            }
        }
    }
}