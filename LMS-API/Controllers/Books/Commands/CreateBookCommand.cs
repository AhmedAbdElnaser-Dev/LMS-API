using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Books.Commands
{
    public class CreateBookCommand

    {
        [Required]
        public string Name { get; set; }  

        [Required]
        public IFormFile PdfFile { get; set; }

        [Required]
        public IFormFile PicFile { get; set; }
    }
}
