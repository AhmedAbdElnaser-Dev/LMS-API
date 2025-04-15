using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Courses.Commands
{
    public class RemoveStudentFromGroupCommand
    {
        public Guid GroupId { get; set; }
        public string StudentId { get; set; }
    }
}
