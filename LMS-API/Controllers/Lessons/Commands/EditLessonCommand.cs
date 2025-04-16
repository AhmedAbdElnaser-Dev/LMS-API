using System;

namespace LMS_API.Controllers.Lessons.Commands
{
    public class EditLessonCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }
}
