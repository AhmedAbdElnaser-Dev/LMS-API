using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Books.ViewModels
{
    public class BookVM
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        [Url]
        public string UrlPdf { get; set; }

        [Url]
        public string UrlPic { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Dictionary<string, BookTranslationVM> Translations { get; set; } = new();
    }
}