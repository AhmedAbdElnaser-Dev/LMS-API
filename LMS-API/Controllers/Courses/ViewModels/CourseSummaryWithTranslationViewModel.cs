namespace LMS_API.Controllers.Courses.ViewModels
{
    public class CourseSummaryWithTranslationViewModel
    {
        public Guid Id { get; set; }
        public string TranslationName { get; set; }
        public string TranslationTitle { get; set; }
        public string CategoryName { get; set; }
        public List<string> BookTranslationNames { get; set; } = new();
    }
}
