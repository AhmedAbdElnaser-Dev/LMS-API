using System;

namespace LMS_API.Controllers.Groups.ViewModels
{
    public class GroupTranslationViewModel
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public string Language { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
