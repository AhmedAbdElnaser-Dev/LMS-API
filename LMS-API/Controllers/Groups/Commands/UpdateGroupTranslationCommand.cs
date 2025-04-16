using System;

namespace LMS_API.Controllers.Groups.Commands
{
    public class UpdateGroupTranslationCommand
    {
        public Guid GroupId { get; set; }
        public string Language { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
