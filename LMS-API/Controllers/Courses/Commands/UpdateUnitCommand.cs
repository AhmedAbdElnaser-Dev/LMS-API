namespace LMS_API.Controllers.Courses.Commands
{
    public class UpdateUnitCommand
    {
        public Guid UnitId { get; set; }
        public List<UnitTranslationCommand> Translations { get; set; } = new();
    }
}
