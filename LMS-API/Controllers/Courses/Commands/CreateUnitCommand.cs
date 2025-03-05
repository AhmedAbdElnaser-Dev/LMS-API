namespace LMS_API.Controllers.Courses.Commands
{
    public class CreateUnitCommand
    {
        public Guid CourseId { get; set; }
        public List<UnitTranslationCommand> Translations { get; set; } = new();
    }
}
