namespace LMS_API.Controllers.Courses.Commands
{
    public class AddCourseBooksCommand
    {
        public Guid CourseId { get; set; }
        public List<Guid> BookIds { get; set; } = new();
    }
}
