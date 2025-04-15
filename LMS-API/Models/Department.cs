using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LMS_API.Models.Enums;

namespace LMS_API.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Department: BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public string SupervisorId { get; set; }

        [ForeignKey("SupervisorId")]
        public ApplicationUser Supervisor { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        public Category Category { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public List<Course> Courses { get; set; } = new();
        public List<DepartmentTranslation> Translations { get; set; } = new();
    }
}
