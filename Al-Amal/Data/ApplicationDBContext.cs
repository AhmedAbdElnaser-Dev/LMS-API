using Al_Amal.Models;
using Microsoft.EntityFrameworkCore;

namespace Al_Amal.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasIndex(u => u.Telegram).IsUnique();
        }

        public DbSet<User> Users { get; set; } 
    }
}
