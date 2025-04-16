using System;
using System.Collections.Generic;
using LMS_API.Controllers.Lessons.ViewModels;

namespace LMS_API.Controllers.Units.ViewModels
{
    public class UnitViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid CourseId { get; set; }
        public List<LessonViewModel> Lessons { get; set; } = new();
    }
}
