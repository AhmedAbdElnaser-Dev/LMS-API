using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class GroupTranslation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid GroupId { get; set; }

        [ForeignKey("GroupId")]
        public Group Group { get; set; }


        [Required]
        [RegularExpression("^(ar|en|ru)$", ErrorMessage = "Language must be 'ar', 'en', or 'ru'")]
        public string Language { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
