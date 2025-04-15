namespace LMS_API.Controllers.Courses.Commands
{
    public class AddStudentToGroupCommand
    {
        public Guid GroupId { get; set; }
        public string StudentId { get; set; }
    }
}
