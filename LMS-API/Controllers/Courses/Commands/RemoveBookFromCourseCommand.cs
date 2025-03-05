namespace LMS_API.Controllers.Courses.Commands
{
    public class RemoveBookFromCourseCommand
    {
        public Guid CourseId { get; set; }
        public Guid BookId { get; set; }
    }
}
