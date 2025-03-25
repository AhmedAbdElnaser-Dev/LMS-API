namespace LMS_API.Controllers.Departments.Commands
{
    public class UpdateCourseBooksCommand
    {
        public Guid CourseId { get; set; }
        public List<Guid> BookIds { get; set; } 
    }
}
