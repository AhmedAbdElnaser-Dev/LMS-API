using System.Runtime;

namespace LMS_API.Controllers.Courses.ViewModels
{
    public class CourseFullDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public List<GroupInfo> Groups { get; set; } = new List<GroupInfo>();
        public List<BookInfo> Books { get; set; } = new List<BookInfo>();
        public List<UnitInfo> Units { get; set; } = new List<UnitInfo>();
        public Dictionary<string, CourseTranslationInfo> Translations { get; set; } = new Dictionary<string, CourseTranslationInfo>();
    }
}
