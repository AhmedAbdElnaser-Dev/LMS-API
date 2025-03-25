using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class Book: BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public List<BookTranslation> Translations { get; set; } = new();

        [Required]
        public string UrlPdf { get; set; }

        [Required]
        public string UrlPic { get; set; }

        public List<CourseBook> CourseBooks { get; set; } = new List<CourseBook>();
    }
}
