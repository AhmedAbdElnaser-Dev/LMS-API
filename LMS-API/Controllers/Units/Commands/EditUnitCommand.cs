using System;

namespace LMS_API.Controllers.Units.Commands
{
    public class EditUnitCommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
