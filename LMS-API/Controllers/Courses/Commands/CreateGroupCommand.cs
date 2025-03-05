namespace LMS_API.Controllers.Courses.Commands
{
    public class CreateGroupCommand
    {
        public Guid CourseId { get; set; }
        public string InstructorId { get; set; }
        public int MaxStudents { get; set; }
        public List<GroupTranslationCommand> Translations { get; set; } = new();
        public List<string> StudentIds { get; set; } = new();
    }
}
