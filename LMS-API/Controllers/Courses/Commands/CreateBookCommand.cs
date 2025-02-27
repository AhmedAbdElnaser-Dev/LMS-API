using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Courses.Commands
{
    public class CreateBookCommand
    {
        [Required]
        [RegularExpression("^(ar|en|ru)$")]
        public string Language { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [Url]
        public string UrlPic { get; set; }

        [Required]
        public string Description { get; set; }

        public string About { get; set; }

        [Url]
        public string DemoUrl { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string Title { get; set; }

        public List<string> Prerequisites { get; set; } = new();
        public List<string> LearningOutcomes { get; set; } = new();
    }
}
