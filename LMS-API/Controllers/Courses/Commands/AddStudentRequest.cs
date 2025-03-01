using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Courses.Commands
{
    public class AddStudentRequest
    {
        [Required]
        [RegularExpression("^(ar|en|ru)$", ErrorMessage = "Language must be 'ar', 'en', or 'ru'")]
        public string Language { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
