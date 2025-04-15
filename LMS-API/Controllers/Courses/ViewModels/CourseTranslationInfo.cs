namespace LMS_API.Controllers.Courses.ViewModels
{
    public class CourseTranslationInfo
    {
        public Guid Id { get; set; } 
        public string Name { get; set; }
        public string UrlPic { get; set; }
        public string Description { get; set; }
        public string About { get; set; }
        public string DemoUrl { get; set; }
        public string Title { get; set; }
        public List<string> Prerequisites { get; set; }
        public List<string> LearningOutcomes { get; set; }
    }
}
