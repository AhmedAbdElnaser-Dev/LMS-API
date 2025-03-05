namespace LMS_API.Controllers.Courses.Commands
{
    public class CreateLessonCommand
    {
        public Guid UnitId { get; set; }
        public string Title { get; set; }
        public List<LessonTranslationCommand> Translations { get; set; } = new();
    }
}
