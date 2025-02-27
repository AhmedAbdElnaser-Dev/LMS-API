namespace LMS_API.Controllers.Books.Commands
{
    public class EditBookTranslationCommand
    {
        public string Language { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
