using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class GroupStudent
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public ApplicationUser Student { get; set; }

        [Required]
        public Guid GroupId { get; set; }

        [ForeignKey("GroupId")]
        public Group Group { get; set; }
    }
}
