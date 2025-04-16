using System;
using System.Collections.Generic;

namespace LMS_API.Controllers.Groups.ViewModels
{
    public class GroupViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentStudents { get; set; }
        public List<GroupTranslationViewModel> Translations { get; set; }
        public string CourseId { get; internal set; }
        public string InstructorId { get; internal set; }
    }
}
