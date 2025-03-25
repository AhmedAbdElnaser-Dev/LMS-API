using System;

namespace LMS_API.Models
{
    public class CourseCategory
    {
        public Guid CourseId { get; set; }
        public Course Course { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
