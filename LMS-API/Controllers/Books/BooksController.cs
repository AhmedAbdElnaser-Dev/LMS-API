    using LMS_API.Controllers.Books.Commands;
    using LMS_API.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    namespace LMS_API.Controllers.Books
    {
        [Route("api/[controller]")]
        [ApiController]
        [Authorize]
        public class BooksController : ControllerBase
        {
            private readonly BookService _bookService;

            public BooksController(BookService bookService)
            {
                _bookService = bookService;
            }

            [HttpGet]
            [Authorize(Roles = "SuperAdmin,Admin,Manager,Supervisor,Teacher,Student")]
            public async Task<IActionResult> GetAllBooks()
            {
                var books = await _bookService.GetAllBooks();
                return Ok(books);
            }

            [HttpGet("{bookId}")]
            [Authorize(Roles = "SuperAdmin,Admin,Manager,Supervisor,Teacher,Student")]
            public async Task<IActionResult> GetBook(Guid bookId)
            {
                var (success, book, error) = await _bookService.GetBookById(bookId);
                if (!success)
                    return NotFound(new { message = error });

                return Ok(book);
            }

            [HttpGet("{bookId}/translations")]
            [Authorize(Roles = "SuperAdmin,Admin,Manager,Supervisor,Teacher,Student")]
            public async Task<IActionResult> GetBookTranslations(Guid bookId)
            {
                var (success, translations, error) = await _bookService.GetBookTranslations(bookId);
                if (!success)
                    return NotFound(new { message = error });

                return Ok(translations);
            }

            [HttpPost("add")]
            [Authorize(Roles = "SuperAdmin,Admin,Manager")]
            public async Task<IActionResult> CreateBook([FromForm] CreateBookCommand command)
            {
                var (success, book, error) = await _bookService.CreateBook(command);
                if (!success)
                    return BadRequest(new { message = error });

                return Ok(book);
            }

            [HttpPost("add-translation")]
            [Authorize(Roles = "SuperAdmin,Admin,Manager,Teacher")]
            public async Task<IActionResult> AddTranslation([FromBody] AddBookTranslationCommand command)
            {
                var requiredPermission = $"Translate_{command.Language}";
                if (!User.IsInRole("SuperAdmin") && !User.HasClaim(c => c.Type == "Permission" && c.Value == requiredPermission))
                    return Forbid();

                var (success, translation, error) = await _bookService.AddBookTranslation(command);
                if (!success)
                    return BadRequest(new { message = error });

                return Ok(new
                {
                    id = translation!.Id,
                    bookId = translation.BookId,
                    language = translation.Language,
                    name = translation.Name,
                    description = translation.Description
                });
            }

            [HttpDelete("{bookId}")]
            [Authorize(Roles = "SuperAdmin,Admin")]
            public async Task<IActionResult> DeleteBook(Guid bookId)
            {
                var (success, error) = await _bookService.DeleteBook(bookId);
                if (!success)
                    return BadRequest(new { message = error });

                return Ok(new { message = "Book deleted successfully" });
            }

            [HttpDelete("delete-translation/{bookTranslationId}")]
            [Authorize(Roles = "SuperAdmin,Admin,Manager")]
            public async Task<IActionResult> DeleteBookTranslation(Guid bookTranslationId)
            {
                var translation = await _bookService.GetTranslationById(bookTranslationId);
                if (translation == null)
                    return NotFound(new { message = "Translation not found" });

                var requiredPermission = $"Translate_{translation.Language}";
                if (!User.IsInRole("SuperAdmin") && !User.HasClaim(c => c.Type == "Permission" && c.Value == requiredPermission))
                    return Forbid();

                var (success, error) = await _bookService.DeleteBookTranslation(bookTranslationId);
                if (!success)
                    return BadRequest(new { message = error });

                return Ok(new { message = "Book translation deleted successfully" });
            }

            [HttpPut("{bookId}/picture")]
            [Authorize(Roles = "SuperAdmin,Admin,Manager,Teacher")]
            [Consumes("multipart/form-data")]
            public async Task<IActionResult> EditBookPicture(Guid bookId, [FromForm] EditBookPictureCommand command)
            {
                var (success, error) = await _bookService.EditBookPicture(bookId, command.Picture);
                if (!success)
                    return BadRequest(new { message = error });

                return Ok(new { message = "Book picture updated successfully" });
            }

            [HttpPut("{bookId}/pdf")]
            [Authorize(Roles = "SuperAdmin,Admin,Manager,Teacher")]
            [Consumes("multipart/form-data")]
            public async Task<IActionResult> EditBookPdf(Guid bookId, [FromForm] EditBookPdfCommand command)
            {
                var (success, error) = await _bookService.EditBookPdf(bookId, command.Pdf);
                if (!success)
                    return BadRequest(new { message = error });

                return Ok(new { message = "Book PDF updated successfully" });
            }

            [HttpPut("update-translation/{bookTranslationId}")]
            [Authorize(Roles = "SuperAdmin,Admin,Manager,Teacher")]
            public async Task<IActionResult> EditBookTranslation(Guid bookTranslationId, [FromBody] EditBookTranslationCommand model)
            {
                var requiredPermission = $"Translate_{model.Language}";
                if (!User.IsInRole("SuperAdmin") && !User.HasClaim(c => c.Type == "Permission" && c.Value == requiredPermission))
                    return Forbid();

                var (success, error) = await _bookService.EditBookTranslation(bookTranslationId, model);
                if (!success)
                    return BadRequest(new { message = error });

                return Ok(new { message = "Book translation updated successfully" });
            }
        }
    }
