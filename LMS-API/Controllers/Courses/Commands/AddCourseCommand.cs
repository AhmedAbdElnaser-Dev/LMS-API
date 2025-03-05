namespace LMS_API.Controllers.Courses.Commands
{
    public class AddCourseCommand
    {
        public Guid CategoryId { get; set; }
        public List<Guid> BookIds { get; set; } = new();
    }
}