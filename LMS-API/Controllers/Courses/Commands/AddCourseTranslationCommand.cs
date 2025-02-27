using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Courses.Commands
{
    public class AddCourseTranslationCommand
    {
        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        [StringLength(255)]
        public string DefaultLanguage { get; set; } = "en";
    }
}
