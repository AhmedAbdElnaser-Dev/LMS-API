using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS_API.Controllers.Lessons.Commands;
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

        public async Task<Lesson> CreateLessonAsync(Guid unitId, string title, string description)
        {
            var lesson = new Lesson
            {
                UnitId = unitId,
                Title = title,
                Description = description
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            return lesson;
        }

        public async Task<Lesson> UpdateLessonAsync(Guid id, string title, string description)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
                throw new KeyNotFoundException($"Lesson with ID {id} not found");

            lesson.Title = title;
            lesson.Description = description;
            _context.Lessons.Update(lesson);


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

        public async Task<LessonTranslation> AddTranslationAsync(AddLessonTranslationCommand command)
        {
            var lesson = await _context.Lessons.Include(l => l.Unit)
                .FirstOrDefaultAsync(l => l.Id == command.LessonId);

            if (lesson == null)
                throw new KeyNotFoundException("Lesson not found");

            var exists = await _context.LessonTranslations
                .AnyAsync(t => t.Title == command.Title && t.Lesson.UnitId == lesson.UnitId);
            if (exists)
                throw new InvalidOperationException("This title already exists for this unit");

            var translation = new LessonTranslation
            {
                LessonId = command.LessonId,
                Language = command.Language,
                Title = command.Title,
                Content = command.Content
            };

            _context.LessonTranslations.Add(translation);
            await _context.SaveChangesAsync();
            return translation;
        }

        public async Task<LessonTranslation> UpdateTranslationAsync(Guid id, EditLessonTranslationCommand command)
        {
            var translation = await _context.LessonTranslations
                .Include(t => t.Lesson)
                .ThenInclude(l => l.Unit)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (translation == null)
                throw new KeyNotFoundException("Translation not found");

            var exists = await _context.LessonTranslations
                .AnyAsync(t => t.Id != id && t.Title == command.Title && t.Lesson.UnitId == translation.Lesson.UnitId);
            if (exists)
                throw new InvalidOperationException("Another translation with this title already exists in the same unit");

            translation.Language = command.Language;
            translation.Title = command.Title;
            translation.Content = command.Content;

            _context.LessonTranslations.Update(translation);
            await _context.SaveChangesAsync();

            return translation;
        }

    }
}
