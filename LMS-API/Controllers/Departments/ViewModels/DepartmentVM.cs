namespace LMS_API.Controllers.Departments.ViewModels
{
    public class DepartmentVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public SupervisorVM Supervisor { get; set; } 
        public string CategoryName { get; set; }     
        public string Gender { get; set; }
        public Dictionary<string, DepartmentTranslationVM> Translations { get; set; } = new Dictionary<string, DepartmentTranslationVM>();
    }
}
