namespace LMS_API.Controllers.Courses.Commands
{
    public class UpdateCourseCategoryCommand
    {
        public Guid CourseId { get; set; }
        public Guid CategoryId { get; set; }
    }
}
