using System;

namespace LMS_API.Controllers.Groups.Commands
{
    public class EditGroupCommand
    {
        public Guid GroupId { get; set; }
        public string Name { get; set; }
        public int MaxStudents { get; set; }
    }
}
