using LMS_API.Data;
using LMS_API.Helpers;
using LMS_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS_API.Services
{
    public interface IUnitService
    {
        Task<Unit> CreateUnitAsync(Guid courseId, string name);
        Task<Unit> UpdateUnitAsync(Guid id, string name);
        Task<Unit> GetUnitAsync(Guid id);
        Task DeleteUnitAsync(Guid id);
    }

    public class UnitService : IUnitService
    {
        private readonly DBContext _context;
        private readonly ILogger<UnitService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UnitService(DBContext context, ILogger<UnitService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Unit> CreateUnitAsync(Guid courseId, string name)
        {
            try
            {
                if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Add_Unit"))
                    throw new UnauthorizedAccessException("Permission denied to add unit");

                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == courseId);
                if (course == null)
                {
                    _logger.LogWarning("Course {CourseId} not found", courseId);
                    throw new ArgumentException("Course not found");
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.LogWarning("Unit name is empty for course {CourseId}", courseId);
                    throw new ArgumentException("Unit name is required");
                }

                var unit = new Unit
                {
                    Id = Guid.NewGuid(),
                    CourseId = courseId,
                    Name = name,
                    Translations = new List<UnitTranslation>()
                };

                await _context.Units.AddAsync(unit);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created unit {UnitId} for course {CourseId}", unit.Id, courseId);
                return unit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating unit for course {CourseId}", courseId);
                throw;
            }
        }

        public async Task<Unit> UpdateUnitAsync(Guid id, string name)
        {
            try
            {
                if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Update_Unit"))
                    throw new UnauthorizedAccessException("Permission denied to update unit");

                var unit = await _context.Units
                    .FirstOrDefaultAsync(u => u.Id == id);
                if (unit == null)
                {
                    _logger.LogWarning("Unit {UnitId} not found", id);
                    throw new ArgumentException("Unit not found");
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.LogWarning("Unit name is empty for unit {UnitId}", id);
                    throw new ArgumentException("Unit name is required");
                }

                unit.Name = name;
                _context.Units.Update(unit);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated unit {UnitId}", id);
                return unit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating unit {UnitId}", id);
                throw;
            }
        }

        public async Task<Unit> GetUnitAsync(Guid id)
        {
            try
            {
                var unit = await _context.Units
                    .Include(u => u.Translations)
                    .FirstOrDefaultAsync(u => u.Id == id);
                if (unit == null)
                {
                    _logger.LogWarning("Unit {UnitId} not found", id);
                    throw new ArgumentException("Unit not found");
                }

                _logger.LogInformation("Retrieved unit {UnitId}", id);
                return unit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unit {UnitId}", id);
                throw;
            }
        }

        public async Task DeleteUnitAsync(Guid id)
        {
            try
            {
                if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Delete_Unit"))
                    throw new UnauthorizedAccessException("Permission denied to delete unit");

                var unit = await _context.Units
                    .FirstOrDefaultAsync(u => u.Id == id);
                if (unit == null)
                {
                    _logger.LogWarning("Unit {UnitId} not found", id);
                    throw new ArgumentException("Unit not found");
                }

                _context.Units.Remove(unit);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted unit {UnitId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting unit {UnitId}", id);
                throw;
            }
        }

        public async Task AddUnitTranslationAsync(Guid unitId, string language, string name)
        {
            var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId);
            if (unit == null)
                throw new ArgumentException("Unit not found");

            if (await _context.UnitTranslations.AnyAsync(t => t.UnitId == unitId && t.Language == language))
                throw new ArgumentException("Translation for this language already exists");

            var translation = new UnitTranslation
            {
                UnitId = unitId,
                Language = language,
                Name = name
            };

            await _context.UnitTranslations.AddAsync(translation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added translation to unit {UnitId} in language {Language}", unitId, language);
        }

        public async Task UpdateUnitTranslationAsync(Guid unitId, string language, string name)
        {
            var translation = await _context.UnitTranslations
                .FirstOrDefaultAsync(t => t.UnitId == unitId && t.Language == language);
            if (translation == null)
                throw new ArgumentException("Translation not found");

            translation.Name = name;
            _context.UnitTranslations.Update(translation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated translation of unit {UnitId} for language {Language}", unitId, language);
        }

    }
}