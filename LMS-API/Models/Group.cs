using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class Group
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

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
    }
}
