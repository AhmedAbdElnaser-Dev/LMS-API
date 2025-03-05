namespace LMS_API.Controllers.Courses.Commands
{
    public class UpdateGroupCommand
    {
        public Guid GroupId { get; set; }
        public string InstructorId { get; set; }
        public int MaxStudents { get; set; }
        public List<GroupTranslationCommand> Translations { get; set; } = new();
    }
}
