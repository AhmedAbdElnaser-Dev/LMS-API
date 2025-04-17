using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Lessons.Commands
{
    public class EditLessonTranslationCommand
    {
        [Required]
        [RegularExpression("^(ar|en|ru)$")]
        public string Language { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
