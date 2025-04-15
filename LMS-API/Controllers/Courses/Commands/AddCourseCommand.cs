namespace LMS_API.Controllers.Courses.Commands
{
    public class AddCourseCommand
    {
        public string Name { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid CategoryId { get; set; }
        public List<Guid> BookIds { get; set; }
    }
}