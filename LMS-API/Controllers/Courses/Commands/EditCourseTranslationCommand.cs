namespace LMS_API.Controllers.Courses.Commands
{
    public class EditCourseTranslationCommand
    {
        public Guid TranslationId { get; set; }
        public string Name { get; set; }
        public string UrlPic { get; set; }
        public string Description { get; set; }
        public string About { get; set; }
        public string DemoUrl { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public List<string> Prerequisites { get; set; } = new();
        public List<string> LearningOutcomes { get; set; } = new();
    }
}
