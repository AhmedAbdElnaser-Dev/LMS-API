namespace LMS_API.Controllers.Departments.Commands
{
    public class CreateDepartmentCommand
    {
        public string Name { get; set; }
        public string CategoryId { get; set; }
        public string SupervisorId { get; set; }
        public string gender { get; set; }
    }
}
