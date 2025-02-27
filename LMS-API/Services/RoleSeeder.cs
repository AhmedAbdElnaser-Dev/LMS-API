using Microsoft.AspNetCore.Identity;
public static class RoleSeeder
{
    public static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "SuperAdmin", "Admin", "Manager", "User", "Supervisor", "Teacher", "Student" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var identityRole = new IdentityRole(role)
                {
                    ConcurrencyStamp = Guid.NewGuid().ToString() 
                };

                await roleManager.CreateAsync(identityRole);
            }
        }
    }
}
