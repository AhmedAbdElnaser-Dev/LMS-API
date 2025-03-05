namespace LMS_API.Controllers.Courses.Queries
{
    public class GetUnitWithTranslationQuery
    {
        public Guid UnitId { get; set; }
        public string Language { get; set; }
    }
}
