namespace LMS_API.Controllers.Courses.ViewModels
{
    public class GroupSummaryWithTranslationViewModel
    {
        public Guid Id { get; set; }
        public string InstructorId { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentStudentCount { get; set; }
        public string TranslationName { get; set; }
    }
}