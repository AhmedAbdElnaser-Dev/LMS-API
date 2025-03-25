using System;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class Level: BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        public Guid? BookId { get; set; }

        public Course Course { get; set; }
        public Book Book { get; set; }
    }
}
