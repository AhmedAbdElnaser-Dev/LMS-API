using System;
using System.Collections.Generic;

namespace LMS_API.Controllers.Lessons.ViewModels
{
    public class LessonViewModel
    {
        public Guid Id { get; set; }
        public Guid UnitId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List<LessonTranslationViewModel> Translations { get; set; }
    }
}
