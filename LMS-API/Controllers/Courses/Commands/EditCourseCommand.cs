using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Courses.Commands
{
    public class EditCourseCommand
    {
        [Required]
        public Guid DepartmentId { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public List<Guid> BookIds { get; set; }
    }
}
