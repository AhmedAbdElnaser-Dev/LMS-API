using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class Permission
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
