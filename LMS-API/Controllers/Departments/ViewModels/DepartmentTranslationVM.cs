namespace LMS_API.Controllers.Departments.ViewModels
{
    public class DepartmentTranslationVM
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
    }
}
