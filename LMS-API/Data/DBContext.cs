using LMS_API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Data
{
    public class DBContext : IdentityDbContext<ApplicationUser>
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookTranslation> BookTranslations { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<UnitTranslation> UnitTranslations { get; set; } 
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonTranslation> LessonTranslations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Convert Gender Enum to String
            builder.Entity<ApplicationUser>()
                   .Property(u => u.Gender)
                   .HasConversion<string>();

            // Course - Levels (One-to-Many)
            builder.Entity<Level>()
                .HasOne(l => l.Course)
                .WithMany(c => c.Levels)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Level - Book (One-to-One)
            builder.Entity<Level>()
                .HasOne(l => l.Book)
                .WithOne()
                .HasForeignKey<Level>(l => l.BookId)
                .OnDelete(DeleteBehavior.SetNull);

            // Course - Category (One-to-Many)
            builder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course - Units (One-to-Many)
            builder.Entity<Unit>()
                .HasOne(u => u.Course)
                .WithMany(c => c.Units)
                .HasForeignKey(u => u.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unit - UnitTranslations (One-to-Many)
            builder.Entity<UnitTranslation>()
                .HasOne(ut => ut.Unit)
                .WithMany(u => u.Translations)
                .HasForeignKey(ut => ut.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unit - Lessons (One-to-Many)
            builder.Entity<Lesson>()
                .HasOne(l => l.Unit)
                .WithMany(u => u.Lessons)
                .HasForeignKey(l => l.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lesson - LessonTranslations (One-to-Many)
            builder.Entity<LessonTranslation>()
                .HasOne(lt => lt.Lesson)
                .WithMany(l => l.Translations)
                .HasForeignKey(lt => lt.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Book - BookTranslations (One-to-Many)
            builder.Entity<BookTranslation>()
                .HasOne(bt => bt.Book)
                .WithMany(b => b.Translations)
                .HasForeignKey(bt => bt.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enforce unique constraint on (Name, Language, BookId)
            builder.Entity<BookTranslation>()
                .HasIndex(bt => new { bt.Name, bt.Language, bt.BookId })
                .IsUnique();

            // Create an index on Name for performance optimization
            builder.Entity<BookTranslation>()
                .HasIndex(bt => bt.Name);

            // Book - BookTranslations (One-to-Many)
            builder.Entity<BookTranslation>()
                .HasOne(bt => bt.Book)
                .WithMany(b => b.Translations)
                .HasForeignKey(bt => bt.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
