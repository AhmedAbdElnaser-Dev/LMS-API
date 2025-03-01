using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LMS_API.Models
{
    public class Course
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AddedBy { get; set; }

        public Guid CategoryId { get; set; }

        public Category Category { get; set; }

        public List<Level> Levels { get; set; } = new();
        public List<Book> Books { get; set; } = new();
        public List<Unit> Units { get; set; } = new();
        public List<Group> Groups { get; set; } = new();
        public List<CourseTranslation> Translations { get; set; } = new();
    }
}
