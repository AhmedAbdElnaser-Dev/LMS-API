using System;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Books.Commands
{
    public class AddBookTranslationCommand
    {
        [Required]
        public Guid BookId { get; set; }

        [Required]
        [RegularExpression("^(ar|en|ru)$", ErrorMessage = "Language must be 'ar', 'en', or 'ru'")]
        public string Language { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
