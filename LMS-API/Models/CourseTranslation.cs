using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LMS_API.Models
{
    public class CourseTranslation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CourseId { get; set; }

        [Required]
        [RegularExpression("^(ar|en|ru)$", ErrorMessage = "Language must be 'ar', 'en', or 'ru'")]
        public string Language { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [Url]
        public string UrlPic { get; set; }

        [Required]
        public string Description { get; set; }

        public string About { get; set; }

        [Url]
        public string DemoUrl { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 255 characters.")]
        public string Title { get; set; }
        public Course Course { get; set; }

        [Required]
        public string PrerequisitesJson { get; set; } = "[]";
        public string LearningOutcomesJson { get; set; } = "[]";

        public List<string> Prerequisites
        {
            get => string.IsNullOrEmpty(PrerequisitesJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(PrerequisitesJson);
            set => PrerequisitesJson = JsonSerializer.Serialize(value);
        }
        public List<string> LearningOutcomes
        {
            get => string.IsNullOrEmpty(LearningOutcomesJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(LearningOutcomesJson);
            set => LearningOutcomesJson = JsonSerializer.Serialize(value);
        }
    }
}
