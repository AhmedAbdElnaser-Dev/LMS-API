namespace LMS_API.Controllers.Departments.ViewModels
{
    public class DepartmentTranslationVM
    {
        public Guid Id { get; set; }
        public Guid DepartmentId { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
    }
}
