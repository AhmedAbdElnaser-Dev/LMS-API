using LMS_API.Controllers.Courses.ViewModels;

namespace LMS_API.Controllers.Courses.Commands
{
    public class GroupDetailsWithTranslationsViewModel
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentStudentCount { get; set; }
        public UserViewModel Instructor { get; set; }
        public List<UserViewModel> Students { get; set; }
        public List<GroupTranslationViewModel> Translations { get; set; }
        public string Name { get; internal set; }
    }
}
