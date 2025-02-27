using LMS_API.Controllers.Books.Commands;
using LMS_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers.Books
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookService _bookService;

        public BooksController(BookService bookService)
        {
            _bookService = bookService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _bookService.GetAllBooks();
            return Ok(books);
        }


        [HttpGet("{bookId}")]
        public async Task<IActionResult> GetBook(Guid bookId)
        {
            var (success, book, error) = await _bookService.GetBookById(bookId);
            if (!success)
                return NotFound(new { message = error });

            return Ok(book);
        }


        [HttpGet("{bookId}/translations")]
        public async Task<IActionResult> GetBookTranslations(Guid bookId)
        {
            var (success, translations, error) = await _bookService.GetBookTranslations(bookId);
            if (!success)
                return NotFound(new { message = error });

            return Ok(translations);
        }


        [HttpPost("add")]
        public async Task<IActionResult> CreateBook([FromForm] CreateBookCommand command)
        {
            var (success, book, error) = await _bookService.CreateBook(command);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(book);
        }


        [HttpPost("add-translation")]
        public async Task<IActionResult> AddTranslation([FromBody] AddBookTranslationCommand command)
        {
            var (success, error) = await _bookService.AddBookTranslation(command);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Translation added successfully" });
        }


        [HttpDelete("{bookId}")]
        public async Task<IActionResult> DeleteBook(Guid bookId)
        {
            var (success, error) = await _bookService.DeleteBook(bookId);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Book deleted successfully" });
        }


        [HttpDelete("delete-translation/{bookTranslationId}")]
        public async Task<IActionResult> DeleteBookTranslation(Guid bookTranslationId)
        {
            var (success, error) = await _bookService.DeleteBookTranslation(bookTranslationId);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Book translation deleted successfully" });
        }


        [HttpPut("{bookId}/picture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> EditBookPicture(Guid bookId, [FromForm] EditBookPictureCommand command)
        {
            var (success, error) = await _bookService.EditBookPicture(bookId, command.Picture);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Book picture updated successfully" });
        }


        [HttpPut("{bookId}/pdf")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> EditBookPdf(Guid bookId, [FromForm] EditBookPdfCommand command)
        {
            var (success, error) = await _bookService.EditBookPdf(bookId, command.Pdf);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Book PDF updated successfully" });
        }


        [HttpPut("update-translation/{bookTranslationId}")]
        public async Task<IActionResult> EditBookTranslation(Guid bookTranslationId, [FromBody] EditBookTranslationCommand model)
        {
            var (success, error) = await _bookService.EditBookTranslation(bookTranslationId, model);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Book translation updated successfully" });
        }
    }
}
