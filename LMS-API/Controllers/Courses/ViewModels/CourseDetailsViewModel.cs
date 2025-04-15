namespace LMS_API.Controllers.Courses.ViewModels
{
    public class CourseDetailsViewModel
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } 
        public string Name { get; set; } 
        public int GroupCount { get; set; }
        public int UnitCount { get; set; }
        public int LessonCount { get; set; }
        public int InstructorCount { get; set; } 
        public List<UserViewModel> Instructors { get; set; } = new List<UserViewModel>(); 
    }
}
