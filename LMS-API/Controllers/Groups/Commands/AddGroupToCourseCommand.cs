using System;

namespace LMS_API.Controllers.Groups.Commands
{
    public class AddGroupToCourseCommand
    {
        public Guid CourseId { get; set; }
        public string InstructorId { get; set; }
        public string Name { get; set; }
        public int MaxStudents { get; set; }
    }
}
