using AutoMapper;
using LMS_API.Controllers.Departments.ViewModels;
using LMS_API.Models;

namespace LMS_API.MappingProfiles
{
    public class DepartmentTranslationsResolver : IValueResolver<Department, DepartmentVM, Dictionary<string, DepartmentTranslationVM>>
    {
        public Dictionary<string, DepartmentTranslationVM> Resolve(
            Department source,
            DepartmentVM destination,
            Dictionary<string, DepartmentTranslationVM> destMember,
            ResolutionContext context)
        {
            var translations = source.Translations.ToDictionary(
                t => t.Language,
                t => new DepartmentTranslationVM
                {
                    Id = t.Id,
                    Name = t.Name,
                    Language = t.Language
                });

            var languages = new[] { "en", "ar", "ru" };
            foreach (var lang in languages)
            {
                if (!translations.ContainsKey(lang))
                {
                    translations[lang] = new DepartmentTranslationVM
                    {
                        Id = Guid.Empty,
                        Name = "",
                        Language = lang
                    };
                }
            }

            return translations;
        }
    }
}