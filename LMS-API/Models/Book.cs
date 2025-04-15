using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Book: BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required] 
        public string Name { get; set; }

        public List<BookTranslation> Translations { get; set; } = new();

        [Required]
        public string UrlPdf { get; set; }

        [Required]
        public string UrlPic { get; set; }

        public List<CourseBook> CourseBooks { get; set; } = new List<CourseBook>();
    }
}
