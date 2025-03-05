namespace LMS_API.Controllers.Courses.Commands
{
    public class UpdateLessonCommand
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; }
        public List<LessonTranslationCommand> Translations { get; set; } = new();
    }
}
