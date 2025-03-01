using LMS_API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LMS_API.Data
{
    public class DBContext : IdentityDbContext<ApplicationUser>
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseTranslation> CoursesTranslations { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookTranslation> BookTranslations { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<UnitTranslation> UnitTranslations { get; set; } 
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonTranslation> LessonTranslations { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DepartmentTranslation> DepartmentTranslations { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<GroupTranslation> GroupsTranslations { get; set; }

        public DbSet<GroupStudent> GroupsStudents { get; set; }

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

            // Course - CourseTranslations (One-to-Many)
            builder.Entity<CourseTranslation>()
                .HasOne(ct => ct.Course)
                .WithMany(c => c.Translations)
                .HasForeignKey(ct => ct.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint on (Name, Language, CourseId)
            builder.Entity<CourseTranslation>()
                .HasIndex(ct => new { ct.Name, ct.Language, ct.CourseId })
                .IsUnique();

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

            // Convert Gender Enum to String
            builder.Entity<Department>()
                   .Property(d => d.Gender)
                   .HasConversion<string>();

            // Department - Category (Many-to-One)
            builder.Entity<Department>()
                .HasOne(d => d.Category)
                .WithMany()
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Department - Supervisor (Many-to-One)
            builder.Entity<Department>()
                .HasOne(d => d.Supervisor)
                .WithMany()
                .HasForeignKey(d => d.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Department - Courses (One-to-Many)
            builder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Department - DepartmentTranslations (One-to-Many)
            builder.Entity<DepartmentTranslation>()
                .HasOne(dt => dt.Department)
                .WithMany(d => d.Translations)
                .HasForeignKey(dt => dt.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint on (Name, Language, DepartmentId)
            builder.Entity<DepartmentTranslation>()
                .HasIndex(dt => new { dt.Name, dt.Language, dt.DepartmentId })
                .IsUnique();

            // Group & Instructor (One-to-Many)
            builder.Entity<Group>()
                .HasOne(g => g.Instructor)
                .WithMany()
                .HasForeignKey(g => g.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Group & Course (One-to-Many)
            builder.Entity<Group>()
                .HasOne(g => g.Course)
                .WithMany()
                .HasForeignKey(g => g.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Group & GroupStudents (One-to-Many)
            builder.Entity<Group>()
                .HasMany(g => g.GroupStudents)
                .WithOne(gs => gs.Group)
                .HasForeignKey(gs => gs.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Group & Translations (One-to-Many)
            builder.Entity<GroupTranslation>()
                .HasOne(gt => gt.Group)
                .WithMany()
                .HasForeignKey(gt => gt.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student & GroupStudent (One-to-Many)
            builder.Entity<GroupStudent>()
                .HasOne(gs => gs.Student)
                .WithMany()
                .HasForeignKey(gs => gs.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Define composite key for GroupTranslation
            builder.Entity<GroupTranslation>()
                .HasKey(gt => new { gt.GroupId, gt.Language });

            // Ensure GroupId is a foreign key with cascading delete
            builder.Entity<GroupTranslation>()
                .HasOne(gt => gt.Group)
                .WithMany()
                .HasForeignKey(gt => gt.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Course to Groups Relationship (One-to-Many)
            builder.Entity<Group>()
                .HasOne(g => g.Course)
                .WithMany(c => c.Groups)
                .HasForeignKey(g => g.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
