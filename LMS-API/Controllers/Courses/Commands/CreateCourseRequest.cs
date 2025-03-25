using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Courses.Commands
{
    public class CreateCourseRequest
    {
        [Required]
        public Guid CategoryId { get; set; }

        public List<string> BookIds { get; set; }
    }
}
