using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class Book
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public List<BookTranslation> Translations { get; set; } = new();

        [Required]
        public string UrlPdf { get; set; }

        [Required]
        public string UrlPic { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
