using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class Unit: BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public List<UnitTranslation> Translations { get; set; } = new();

        [Required]
        public Guid CourseId { get; set; }
        public Course Course { get; set; }

        public List<Lesson> Lessons { get; set; } = new();
    }
}
