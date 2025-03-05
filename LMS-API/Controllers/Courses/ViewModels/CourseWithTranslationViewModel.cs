namespace LMS_API.Controllers.Courses.ViewModels
{
    public class CourseWithTranslationViewModel
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } // Assuming Category has a Name property
        public string AddedBy { get; set; } // User ID who added the course
        public string TranslationName { get; set; }
        public string TranslationUrlPic { get; set; }
        public string TranslationDescription { get; set; }
        public string TranslationAbout { get; set; }
        public string TranslationDemoUrl { get; set; }
        public string TranslationTitle { get; set; }
        public List<string> TranslationPrerequisites { get; set; }
        public List<string> TranslationLearningOutcomes { get; set; }
        public List<BookTranslationViewModel> Books { get; set; } = new();
    }
}
