﻿namespace LMS_API.Controllers.Courses.Queries
{
    public class GetCourseWithTranslationQuery
    {
        public Guid CourseId { get; set; }
        public string Language { get; set; }
    }
}
