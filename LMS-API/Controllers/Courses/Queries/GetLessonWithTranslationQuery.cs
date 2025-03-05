namespace LMS_API.Controllers.Courses.Queries
{
    public class GetLessonWithTranslationQuery
    {
        public Guid LessonId { get; set; }
        public string Language { get; set; }
    }
}
