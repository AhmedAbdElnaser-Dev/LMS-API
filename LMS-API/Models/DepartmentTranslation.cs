using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class DepartmentTranslation: BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DepartmentId { get; set; }

        public Department Department { get; set; }

        [Required]
        [RegularExpression("^(ar|en|ru)$", ErrorMessage = "Language must be 'ar', 'en', or 'ru'")]
        public string Language { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }
    }
}
