using System;

namespace LMS_API.Controllers.Lessons.ViewModels
{
    public class LessonTranslationViewModel
    {
        public Guid Id { get; set; }
        public string Language { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
