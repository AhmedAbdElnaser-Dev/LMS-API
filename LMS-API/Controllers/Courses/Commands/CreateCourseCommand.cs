using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Courses.Commands
{
    public class CreateCourseCommand
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Url]
        public string UrlPdf { get; set; }

        [Url]
        public string UrlPic { get; set; }
    }
}
