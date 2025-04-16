using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS_API.Data;
using LMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services
{
    public class UnitService
    {
        private readonly DBContext _context;

        public UnitService(DBContext context)
        {
            _context = context;
        }

        public async Task<Unit> CreateUnitAsync(Guid courseId, string name, string description)
        {
            var unit = new Unit
            {
                CourseId = courseId
            };

            var translation = new UnitTranslation
            {
                Name = name,
                //Description = description,
                //LanguageId = "en",
                UnitId = unit.Id
            };

            unit.Translations.Add(translation);
            _context.Units.Add(unit);
            await _context.SaveChangesAsync();
            return unit;
        }

        public async Task<Unit> UpdateUnitAsync(Guid id, string name, string description)
        {
            var unit = await _context.Units
                .Include(u => u.Translations)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null)
                throw new KeyNotFoundException($"Unit with ID {id} not found");

            var translation = unit.Translations.FirstOrDefault(t => t.Language == "en");
            if (translation != null)
            {
                translation.Name = name;
            }

            await _context.SaveChangesAsync();
            return unit;
        }

        public async Task<Unit> GetUnitAsync(Guid id)
        {
            var unit = await _context.Units
                .Include(u => u.Translations)
                .Include(u => u.Lessons)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null)
                throw new KeyNotFoundException($"Unit with ID {id} not found");

            return unit;
        }

        public async Task DeleteUnitAsync(Guid id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null)
                throw new KeyNotFoundException($"Unit with ID {id} not found");

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();
        }
    }
}
