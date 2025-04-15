using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace LMS_API.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Course: BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        public Category Category { get; set; }

        [ForeignKey("Department")]
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; }

        public List<Level> Levels { get; set; } = new();
        public List<CourseBook> CourseBooks { get; set; } = new List<CourseBook>();
        public List<Unit> Units { get; set; } = new();
        public List<Group> Groups { get; set; } = new();
        public List<CourseTranslation> Translations { get; set; } = new();
    }
}
