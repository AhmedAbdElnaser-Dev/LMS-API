using System;

namespace LMS_API.Controllers.Lessons.Commands
{
    public class AddLessonCommand
    {
        public Guid UnitId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }
}
