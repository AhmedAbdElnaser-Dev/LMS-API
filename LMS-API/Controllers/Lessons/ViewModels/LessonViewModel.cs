using System;

namespace LMS_API.Controllers.Lessons.ViewModels
{
    public class LessonViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public Guid UnitId { get; set; }
    }
}
