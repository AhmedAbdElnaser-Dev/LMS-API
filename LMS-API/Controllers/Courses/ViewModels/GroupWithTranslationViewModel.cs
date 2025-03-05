namespace LMS_API.Controllers.Courses.ViewModels
{
    public class GroupWithTranslationViewModel
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string InstructorId { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentStudentCount { get; set; }
        public List<string> StudentIds { get; set; } = new();
        public string TranslationName { get; set; }
        public string TranslationDescription { get; set; }
    }
}
