using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS_API.Data;
using LMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services
{
    public class LessonService
    {
        private readonly DBContext _context;

        public LessonService(DBContext context)
        {
            _context = context;
        }

        public async Task<Lesson> CreateLessonAsync(Guid unitId, string name, string description, string content)
        {
            var lesson = new Lesson
            {
                UnitId = unitId
            };

            var translation = new LessonTranslation
            {
                //Name = name,
                //Description = description,
                //Content = content,
                //LanguageId = "en", // Default language
                //LessonId = lesson.Id
            };

            lesson.Translations.Add(translation);
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            return lesson;
        }

        public async Task<Lesson> UpdateLessonAsync(Guid id, string name, string description, string content)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Translations)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
                throw new KeyNotFoundException($"Lesson with ID {id} not found");

            var translation = lesson.Translations.FirstOrDefault(t => t.Language == "en");
            if (translation != null)
            {
                //translation.Name = name;
                //translation.Description = description;
                translation.Content = content;
            }

            await _context.SaveChangesAsync();
            return lesson;
        }

        public async Task<Lesson> GetLessonAsync(Guid id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Translations)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
                throw new KeyNotFoundException($"Lesson with ID {id} not found");

            return lesson;
        }

        public async Task DeleteLessonAsync(Guid id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
                throw new KeyNotFoundException($"Lesson with ID {id} not found");

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
        }
    }
}
