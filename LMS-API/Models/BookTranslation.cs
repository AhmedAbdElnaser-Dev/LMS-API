using System;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class BookTranslation: BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [RegularExpression("^(ar|en|ru)$", ErrorMessage = "Language must be 'ar', 'en', or 'ru'")]
        public string Language { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public Guid BookId { get; set; }
        public Book Book { get; set; }
    }
}
