using LMS_API.Data;
using LMS_API.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

public static class CategorySeeder
{
    public static async Task SeedCategories(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DBContext>();

        if (!context.Categories.Any()) 
        {
            var categories = new[]
            {
                new Category { Id = Guid.NewGuid(), Name = "Quraan" },
                new Category { Id = Guid.NewGuid(), Name = "Tajweed" },
                new Category { Id = Guid.NewGuid(), Name = "Arabic" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }
}
