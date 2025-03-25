namespace LMS_API.Controllers.Departments.Commands
{
    public class UpdateCourseCategoryCommand
    {
        public Guid CourseId { get; set; }
        public Guid CategoryId { get; set; }
    }
}
