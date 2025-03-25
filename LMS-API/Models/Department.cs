using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public enum Gender
    {
        Male,
        Female,
        Kids
    }

    public class Department: BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string SupervisorId { get; set; }

        [ForeignKey("SupervisorId")]
        public ApplicationUser Supervisor { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        public Category Category { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public List<Course> Courses { get; set; } = new();
        public List<DepartmentTranslation> Translations { get; set; } = new();
    }
}
