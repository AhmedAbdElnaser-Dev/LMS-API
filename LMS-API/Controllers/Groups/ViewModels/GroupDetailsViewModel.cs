using LMS_API.Controllers.Courses.ViewModels;

namespace LMS_API.Controllers.Groups.ViewModels
{
    public class GroupDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int MaxStudents { get; set; }
        public Guid CourseId { get; set; }
        public CourseViewModel Course { get; set; }
        public string InstructorId { get; set; }
        public InstructorViewModel Instructor { get; set; }
        public List<StudentViewModel> Students { get; set; }
        public List<GroupTranslationViewModel> Translations { get; set; }
    }
}
