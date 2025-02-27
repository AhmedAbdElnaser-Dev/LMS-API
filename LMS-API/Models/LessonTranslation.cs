using System;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class LessonTranslation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [RegularExpression("^(ar|en|ru)$", ErrorMessage = "Language must be 'ar', 'en', or 'ru'")]
        public string Language { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public Guid LessonId { get; set; }
        public Lesson Lesson { get; set; }
    }
}
