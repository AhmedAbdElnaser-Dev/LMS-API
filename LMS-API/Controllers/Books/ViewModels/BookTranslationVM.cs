namespace LMS_API.Controllers.Books.ViewModels
{
    public class BookTranslationVM
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
