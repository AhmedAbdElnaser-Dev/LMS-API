using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class Lesson: BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public Guid UnitId { get; set; }
        public Unit Unit { get; set; }

        public List<LessonTranslation> Translations { get; set; } = new(); 
    }
}
