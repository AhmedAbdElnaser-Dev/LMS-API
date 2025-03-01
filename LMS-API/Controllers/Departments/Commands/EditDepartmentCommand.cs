namespace LMS_API.Controllers.Departments.Commands
{
    public class EditDepartmentCommand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CategoryId { get; set; }
        public string SupervisorId { get; set; }
    }
}
