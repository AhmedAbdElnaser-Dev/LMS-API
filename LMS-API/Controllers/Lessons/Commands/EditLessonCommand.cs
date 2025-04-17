using System;

namespace LMS_API.Controllers.Lessons.Commands
{
    public class EditLessonCommand
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
