using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class UnitTranslation:  BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UnitId { get; set; }

        [Required]
        [RegularExpression("^(ar|en|ru)$", ErrorMessage = "Language must be 'ar', 'en', or 'ru'")]
        public string Language { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public Unit Unit { get; set; }
    }
}
