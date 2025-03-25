using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Books.ViewModels
{
    public class BookVM
    {
        public Guid Id { get; set; }
        [Url]
        public string UrlPdf { get; set; }

        [Url]
        public string UrlPic { get; set; }

        public string CreatedBy { get; set; }

        public DateTime AddedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
