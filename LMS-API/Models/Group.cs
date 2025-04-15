using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Group: BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public string InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public ApplicationUser Instructor { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public ICollection<GroupStudent> GroupStudents { get; set; } = new List<GroupStudent>();

        [Required]
        public int MaxStudents { get; set; }

        public List<GroupTranslation> Translations { get; set; } = new();
    }
}
