namespace LMS_API.Controllers.Courses.ViewModels
{
    public class UnitSummaryWithTranslationViewModel
    {
        public Guid Id { get; set; }
        public string TranslationName { get; set; }
        public int LessonCount { get; set; }
    }
}
