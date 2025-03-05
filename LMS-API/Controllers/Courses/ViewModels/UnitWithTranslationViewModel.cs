namespace LMS_API.Controllers.Courses.ViewModels
{
    public class UnitWithTranslationViewModel
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string TranslationName { get; set; }
        public int LessonCount { get; set; }
    }
}
