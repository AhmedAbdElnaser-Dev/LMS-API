using System.Security.Claims;
using AutoMapper;
using LMS_API.Controllers.Books.Commands;
using LMS_API.Controllers.Books.ViewModels;
using LMS_API.Controllers.Departments.Commands;
using LMS_API.Data;
using LMS_API.Migrations;
using LMS_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class BookService
{
    private readonly IMapper _mapper;
    private readonly DBContext _context;
    private readonly ILogger<BookService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor; 

    public BookService(
        IMapper mapper,
        DBContext context,
        ILogger<BookService> logger,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _mapper = mapper;
        _context = context;
        _logger = logger;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<BookVM>> GetAllBooks()
    {
        var books = await _context.Books.ToListAsync();
        return _mapper.Map<List<BookVM>>(books);
    }

    public async Task<(bool Success, BookVM? Book, string? Error)> GetBookById(Guid bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
            return (false, null, "Book not found");

        var bookVM = _mapper.Map<BookVM>(book);
        return (true, bookVM, null);
    }

    public async Task<(bool Success, List<BookTranslationVM>? Translations, string? Error)> GetBookTranslations(Guid bookId)
    {
        var translations = await _context.BookTranslations
            .Where(bt => bt.BookId == bookId)
            .ToListAsync();

        if (!translations.Any())
            return (false, null, "No translations found for this book");

        return (true, _mapper.Map<List<BookTranslationVM>>(translations), null);
    }

    public async Task<(bool Success, BookVM? Book, string? Error)> CreateBook(CreateBookCommand command)
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return (false, null, "Invalid or missing token");

            Guid bookId = Guid.NewGuid();
            string? pdfPath = null;
            string? picPath = null;

            if (command.PdfFile != null)
            {
                var pdfExtension = Path.GetExtension(command.PdfFile.FileName).ToLowerInvariant();
                var pdfContentType = command.PdfFile.ContentType.ToLower();

                if (pdfExtension != ".pdf" || pdfContentType != "application/pdf")
                    return (false, null, "Invalid file format. Only PDF files are allowed.");

                pdfPath = await FileHelper.UploadFileAsync(command.PdfFile, "books", bookId.ToString(), "book.pdf");
            }

            if (command.PicFile != null)
            {
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var picExtension = Path.GetExtension(command.PicFile.FileName).ToLowerInvariant();
                var picContentType = command.PicFile.ContentType.ToLower();

                if (!allowedImageExtensions.Contains(picExtension) || !picContentType.StartsWith("image/"))
                    return (false, null, "Invalid file format. Only JPG, PNG, and GIF are allowed for pictures.");

                picPath = await FileHelper.UploadFileAsync(command.PicFile, "books", bookId.ToString(), "book.jpg");
            }

            var book = _mapper.Map<Book>(command);
            book.Id = bookId;
            book.UrlPdf = pdfPath;
            book.UrlPic = picPath;
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;
            book.CreatedBy = userId;
            book.UpdatedBy = userId;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var bookVM = _mapper.Map<BookVM>(book);
            return (true, bookVM, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating book");
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> AddBookTranslation(AddBookTranslationCommand command)
    {
        try
        {
            var book = await _context.Books.FindAsync(command.BookId);
            if (book == null)
                return (false, "Book not found");

            var existingTranslation = await _context.BookTranslations
                .FirstOrDefaultAsync(bt => bt.BookId == command.BookId && bt.Language == command.Language);
            if (existingTranslation != null)
                return (false, "Translation for this language already exists");

            var bookTranslation = new BookTranslation
            {
                Id = Guid.NewGuid(),
                BookId = command.BookId,
                Language = command.Language,
                Name = command.Name,
                Description = command.Description
            };

            _context.BookTranslations.Add(bookTranslation);
            await _context.SaveChangesAsync();

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding book translation");
            return (false, "An error occurred while adding translation");
        }
    }

    public async Task<(bool Success, string? Error)> DeleteBook(Guid bookId)
    {
        try
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return (false, "Book not found");

            // Delete associated files
            if (!string.IsNullOrEmpty(book.UrlPdf))
                FileHelper.DeleteFile(book.UrlPdf);

            if (!string.IsNullOrEmpty(book.UrlPic))
                FileHelper.DeleteFile(book.UrlPic);

            // Remove translations
            var translations = _context.BookTranslations.Where(bt => bt.BookId == bookId);
            _context.BookTranslations.RemoveRange(translations);

            // Remove the book
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book");
            return (false, "An error occurred while deleting the book");
        }
    }

    public async Task<(bool Success, string? Error)> DeleteBookTranslation(Guid bookTranslationId)
    {
        try
        {
            var bookTranslation = await _context.BookTranslations.FindAsync(bookTranslationId);
            if (bookTranslation == null)
                return (false, "Book translation not found");

            _context.BookTranslations.Remove(bookTranslation);
            await _context.SaveChangesAsync();

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book translation");
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> EditBookPicture(Guid bookId, IFormFile picture)
    {
        try
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(picture.FileName).ToLowerInvariant();
            var contentType = picture.ContentType.ToLower();

            if (!allowedExtensions.Contains(extension) || !contentType.StartsWith("image/"))
                return (false, "Invalid file format. Only JPG, PNG, and GIF are allowed for pictures.");

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return (false, "Book not found");

            string picPath = await FileHelper.UploadFileAsync(picture, "books", bookId.ToString(), "book.jpg");
            book.UrlPic = picPath;
            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing book picture");
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> EditBookPdf(Guid bookId, IFormFile pdf)
    {
        try
        {
            // Validate file type (only PDFs allowed)
            var allowedExtensions = new[] { ".pdf" };
            var extension = Path.GetExtension(pdf.FileName).ToLowerInvariant();
            var contentType = pdf.ContentType.ToLower();

            if (!allowedExtensions.Contains(extension) || contentType != "application/pdf")
                return (false, "Invalid file format. Only PDF files are allowed.");

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return (false, "Book not found");

            string pdfPath = await FileHelper.UploadFileAsync(pdf, "books", bookId.ToString(), "book.pdf");
            book.UrlPdf = pdfPath;
            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing book PDF");
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> EditBookTranslation(Guid bookTranslationId, EditBookTranslationCommand model)
    {
        try
        {
            var bookTranslation = await _context.BookTranslations.FindAsync(bookTranslationId);
            if (bookTranslation == null)
                return (false, "Book translation not found");

            bookTranslation.Name = model.Name;
            bookTranslation.Description = model.Description;
            bookTranslation.Language = model.Language;

            await _context.SaveChangesAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing book translation");
            return (false, ex.Message);
        }
    }

}
