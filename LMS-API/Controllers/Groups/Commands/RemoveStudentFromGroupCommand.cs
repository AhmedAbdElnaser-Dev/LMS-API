using System;

namespace LMS_API.Controllers.Groups.Commands
{
    public class RemoveStudentFromGroupCommand
    {
        public Guid GroupId { get; set; }
        public string StudentId { get; set; }
    }
}
