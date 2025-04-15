namespace LMS_API.Controllers.Courses.Commands
{
    public class GroupSummaryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string InstructorId { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentStudentCount { get; set; }
    }
}
