namespace LMS_API.Controllers.Courses.Commands
{
    public class UpdateGroupCommand
    {
        public Guid GroupId { get; set; }
        public string Name { get; set; }    
        public string InstructorId { get; set; }
        public int MaxStudents { get; set; }
    }
}
