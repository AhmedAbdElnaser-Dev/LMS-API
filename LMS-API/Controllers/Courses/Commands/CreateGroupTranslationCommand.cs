namespace LMS_API.Controllers.Courses.Commands
{
    public class CreateGroupTranslationCommand
    {
        public Guid GroupId { get; set; }
        public string Language { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
