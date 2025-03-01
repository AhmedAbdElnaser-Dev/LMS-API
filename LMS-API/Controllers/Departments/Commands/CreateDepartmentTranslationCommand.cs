namespace LMS_API.Controllers.Departments.Commands
{
    public class CreateDepartmentTranslationCommand
    {
        public string DepartmentId { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
    }
}
