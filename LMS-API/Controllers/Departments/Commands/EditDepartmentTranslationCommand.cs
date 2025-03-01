namespace LMS_API.Controllers.Departments.Commands
{
    public class EditDepartmentTranslationCommand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
    }
}
