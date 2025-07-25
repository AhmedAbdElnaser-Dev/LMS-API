using System;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Groups.Commands
{
    public class AddGroupTranslationCommand
    {
        public Guid GroupId { get; set; }
        [Required]
        [RegularExpression("^(ar|en|ru)$", ErrorMessage = "Language must be 'ar', 'en', or 'ru'")]
        public string Language { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
