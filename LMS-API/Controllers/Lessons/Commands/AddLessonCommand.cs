using System;

namespace LMS_API.Controllers.Lessons.Commands
{
    public class AddLessonCommand
    {
        public Guid UnitId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
