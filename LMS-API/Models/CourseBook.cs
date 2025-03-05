namespace LMS_API.Models
{
    public class CourseBook
    {
        public Guid CourseId { get; set; }
        public Course Course { get; set; }

        public Guid BookId { get; set; }
        public Book Book { get; set; }
    }
}
