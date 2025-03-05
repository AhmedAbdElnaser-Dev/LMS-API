using LMS_API.Controllers.Courses.Commands;
using LMS_API.Controllers.Courses.Queries;
using LMS_API.Controllers.Courses.ViewModels;
using LMS_API.Data;
using LMS_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS_API.Services
{
    public class CourseService
    {
        private readonly DBContext _context;
        private readonly ILogger<CourseService> _logger;

        public CourseService(DBContext context, ILogger<CourseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, Course? Course, string? ErrorMessage)> AddCourseAsync(string addedByUserId, AddCourseCommand command)
        {
            try
            {
                if (string.IsNullOrEmpty(addedByUserId))
                    return (false, null, "User ID is required.");

                if (!await _context.Categories.AnyAsync(c => c.Id == command.CategoryId))
                    return (false, null, "Invalid Category ID.");

                if (command.BookIds == null || !command.BookIds.Any())
                    return (false, null, "At least one Book ID is required.");

                var books = await _context.Books
                    .Where(b => command.BookIds.Contains(b.Id))
                    .ToListAsync();

                if (books.Count != command.BookIds.Count)
                {
                    var missingIds = command.BookIds.Except(books.Select(b => b.Id)).ToList();
                    return (false, null, $"The following Book IDs do not exist: {string.Join(", ", missingIds)}");
                }

                var course = new Course
                {
                    AddedBy = Guid.Parse(addedByUserId),
                    CategoryId = command.CategoryId
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync(); // Save course to generate Id

                // Add books via CourseBook
                var CoursesBooks = books.Select(b => new CourseBook
                {
                    CourseId = course.Id,
                    BookId = b.Id
                }).ToList();

                _context.CoursesBooks.AddRange(CoursesBooks); // Fixed typo: CoursesBooks -> CoursesBooks
                await _context.SaveChangesAsync();

                _logger.LogInformation("Course {CourseId} added by user {UserId} with {BookCount} books", course.Id, addedByUserId, books.Count);
                return (true, course, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding course by user {UserId}", addedByUserId);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<bool> UpdateCourseCategoryAsync(UpdateCourseCategoryCommand command)
        {
            try
            {
                var course = await _context.Courses.FindAsync(command.CourseId);
                if (course == null)
                    return false;

                if (!await _context.Categories.AnyAsync(c => c.Id == command.CategoryId))
                    return false;

                course.CategoryId = command.CategoryId;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Course {CourseId} category updated to {CategoryId}", command.CourseId, command.CategoryId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course category for CourseId {CourseId}", command.CourseId);
                return false;
            }
        }

        public async Task<bool> UpdateCoursesBooksAsync(UpdateCoursesBooksCommand command)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.CourseBooks)
                    .FirstOrDefaultAsync(c => c.Id == command.CourseId);

                if (course == null)
                    return false;

                var books = await _context.Books
                    .Where(b => command.BookIds.Contains(b.Id))
                    .ToListAsync();

                if (books.Count != command.BookIds.Count)
                {
                    var missingIds = command.BookIds.Except(books.Select(b => b.Id)).ToList();
                    _logger.LogWarning("Some books were not found: {MissingIds}", string.Join(", ", missingIds));
                    return false;
                }

                // Remove existing CourseBook entries
                _context.CoursesBooks.RemoveRange(course.CourseBooks); // Fixed typo: CoursesBooks -> CoursesBooks
                course.CourseBooks.Clear();

                // Add new CourseBook entries
                var newCoursesBooks = books.Select(b => new CourseBook
                {
                    CourseId = command.CourseId,
                    BookId = b.Id
                }).ToList();

                _context.CoursesBooks.AddRange(newCoursesBooks); // Fixed typo: CoursesBooks -> CoursesBooks
                await _context.SaveChangesAsync();

                _logger.LogInformation("Course {CourseId} books updated with {BookCount} books", command.CourseId, books.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating books for CourseId {CourseId}", command.CourseId);
                return false;
            }
        }

        public async Task<bool> RemoveBookFromCourseAsync(RemoveBookFromCourseCommand command)
        {
            try
            {
                var courseBook = await _context.CoursesBooks 
                    .FirstOrDefaultAsync(cb => cb.CourseId == command.CourseId && cb.BookId == command.BookId);

                if (courseBook == null)
                    return false;

                _context.CoursesBooks.Remove(courseBook);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Book {BookId} removed from Course {CourseId}", command.BookId, command.CourseId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing book {BookId} from CourseId {CourseId}", command.BookId, command.CourseId);
                return false;
            }
        }

        public async Task<(bool Success, CourseTranslation? Translation, string? ErrorMessage)> AddCourseTranslationAsync(AddCourseTranslationCommand command)
        {
            try
            {
                if (!await _context.Courses.AnyAsync(c => c.Id == command.CourseId))
                    return (false, null, "Invalid Course ID.");

                var validLanguages = new[] { "ar", "en", "ru" };
                if (!validLanguages.Contains(command.Language))
                    return (false, null, $"Invalid language: {command.Language}. Must be 'ar', 'en', or 'ru'.");

                if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Length > 255)
                    return (false, null, "Name is required and must be 255 characters or less.");

                if (string.IsNullOrWhiteSpace(command.UrlPic) || !Uri.TryCreate(command.UrlPic, UriKind.Absolute, out _))
                    return (false, null, "Valid URL for UrlPic is required.");

                if (string.IsNullOrWhiteSpace(command.Description))
                    return (false, null, "Description is required.");

                if (string.IsNullOrWhiteSpace(command.Title) || command.Title.Length < 3 || command.Title.Length > 255)
                    return (false, null, "Title must be between 3 and 255 characters.");

                // Check unique constraint (Name, Language, CourseId)
                if (await _context.CoursesTranslations.AnyAsync(ct => ct.Name == command.Name && ct.Language == command.Language && ct.CourseId == command.CourseId))
                    return (false, null, $"A translation with Name '{command.Name}' and Language '{command.Language}' already exists for this course.");

                var translation = new CourseTranslation
                {
                    CourseId = command.CourseId,
                    Language = command.Language,
                    Name = command.Name,
                    UrlPic = command.UrlPic,
                    Description = command.Description,
                    About = command.About,
                    DemoUrl = command.DemoUrl,
                    Title = command.Title,
                    Prerequisites = command.Prerequisites, // JSON serialization handled by property
                    LearningOutcomes = command.LearningOutcomes // JSON serialization handled by property
                };

                _context.CoursesTranslations.Add(translation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Translation {TranslationId} added for Course {CourseId}", translation.Id, command.CourseId);
                return (true, translation, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding translation for CourseId {CourseId}", command.CourseId);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, CourseTranslation? Translation, string? ErrorMessage)> EditCourseTranslationAsync(EditCourseTranslationCommand command)
        {
            try
            {
                var translation = await _context.CoursesTranslations
                    .FirstOrDefaultAsync(ct => ct.Id == command.TranslationId);

                if (translation == null)
                    return (false, null, "Translation not found.");

                if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Length > 255)
                    return (false, null, "Name is required and must be 255 characters or less.");

                if (string.IsNullOrWhiteSpace(command.UrlPic) || !Uri.TryCreate(command.UrlPic, UriKind.Absolute, out _))
                    return (false, null, "Valid URL for UrlPic is required.");

                if (string.IsNullOrWhiteSpace(command.Description))
                    return (false, null, "Description is required.");

                if (string.IsNullOrWhiteSpace(command.Title) || command.Title.Length < 3 || command.Title.Length > 255)
                    return (false, null, "Title must be between 3 and 255 characters.");

                // Check unique constraint (Name, Language, CourseId), excluding current translation
                if (await _context.CoursesTranslations.AnyAsync(ct => ct.Name == command.Name && ct.Language == translation.Language && ct.CourseId == translation.CourseId && ct.Id != command.TranslationId))
                    return (false, null, $"A translation with Name '{command.Name}' and Language '{translation.Language}' already exists for this course.");

                // Update fields
                translation.Name = command.Name;
                translation.UrlPic = command.UrlPic;
                translation.Description = command.Description;
                translation.About = command.About;
                translation.DemoUrl = command.DemoUrl;
                translation.Title = command.Title;
                translation.Prerequisites = command.Prerequisites;
                translation.LearningOutcomes = command.LearningOutcomes;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Translation {TranslationId} updated for Course {CourseId}", translation.Id, translation.CourseId);
                return (true, translation, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing translation {TranslationId}", command.TranslationId);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<bool> DeleteCourseTranslationAsync(DeleteCourseTranslationCommand command)
        {
            try
            {
                var translation = await _context.CoursesTranslations
                    .FirstOrDefaultAsync(ct => ct.Id == command.TranslationId);

                if (translation == null)
                    return false;

                _context.CoursesTranslations.Remove(translation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Translation {TranslationId} deleted for Course {CourseId}", command.TranslationId, translation.CourseId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting translation {TranslationId}", command.TranslationId);
                return false;
            }
        }

        public async Task<(bool Success, CourseWithTranslationViewModel? Course, string? ErrorMessage)> GetCourseWithTranslationAsync(GetCourseWithTranslationQuery command)
        {
            try
            {
                var validLanguages = new[] { "ar", "en", "ru" };
                if (!validLanguages.Contains(command.Language))
                    return (false, null, $"Invalid language: {command.Language}. Must be 'ar', 'en', or 'ru'.");

                var course = await _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.Translations)
                    .Include(c => c.CourseBooks)
                        .ThenInclude(cb => cb.Book)
                        .ThenInclude(b => b.Translations)
                    .FirstOrDefaultAsync(c => c.Id == command.CourseId);

                if (course == null)
                    return (false, null, "Course not found.");

                var translation = course.Translations.FirstOrDefault(t => t.Language == command.Language);
                if (translation == null)
                    return (false, null, $"No translation found for language '{command.Language}'.");

                var viewModel = new CourseWithTranslationViewModel
                {
                    Id = course.Id,
                    CategoryId = course.CategoryId,
                    CategoryName = course.Category?.Name ?? "Unknown",
                    AddedBy = course.AddedBy.ToString(),
                    TranslationName = translation.Name,
                    TranslationUrlPic = translation.UrlPic,
                    TranslationDescription = translation.Description,
                    TranslationAbout = translation.About,
                    TranslationDemoUrl = translation.DemoUrl,
                    TranslationTitle = translation.Title,
                    TranslationPrerequisites = translation.Prerequisites,
                    TranslationLearningOutcomes = translation.LearningOutcomes,
                    Books = course.CourseBooks
                        .Select(cb => cb.Book.Translations.FirstOrDefault(bt => bt.Language == command.Language))
                        .Where(bt => bt != null)
                        .Select(bt => new BookTranslationViewModel { Name = bt.Name })
                        .ToList()
                };

                _logger.LogInformation("Retrieved course {CourseId} with translation in {Language}", command.CourseId, command.Language);
                return (true, viewModel, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course {CourseId} with translation in {Language}", command.CourseId, command.Language);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, List<CourseSummaryWithTranslationViewModel> Courses, string? ErrorMessage)> GetAllCoursesWithTranslationAsync(GetAllCoursesWithTranslationQuery command)
        {
            try
            {
                var validLanguages = new[] { "ar", "en", "ru" };
                if (!validLanguages.Contains(command.Language))
                    return (false, null, $"Invalid language: {command.Language}. Must be 'ar', 'en', or 'ru'.");

                var courses = await _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.Translations)
                    .Include(c => c.CourseBooks)
                        .ThenInclude(cb => cb.Book)
                        .ThenInclude(b => b.Translations)
                    .Where(c => c.Translations.Any(t => t.Language == command.Language))
                    .ToListAsync();

                var viewModels = courses.Select(c =>
                {
                    var translation = c.Translations.First(t => t.Language == command.Language);
                    return new CourseSummaryWithTranslationViewModel
                    {
                        Id = c.Id,
                        TranslationName = translation.Name,
                        TranslationTitle = translation.Title,
                        CategoryName = c.Category?.Name ?? "Unknown", 
                        BookTranslationNames = c.CourseBooks
                            .Select(cb => cb.Book.Translations.FirstOrDefault(bt => bt.Language == command.Language)?.Name)
                            .Where(name => name != null)
                            .ToList()
                    };
                }).ToList();

                _logger.LogInformation("Retrieved {CourseCount} courses with translations in {Language}", viewModels.Count, command.Language);
                return (true, viewModels, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all courses with translation in {Language}", command.Language);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, Unit? Unit, string? ErrorMessage)> CreateUnitAsync(CreateUnitCommand command)
        {
            try
            {
                if (!await _context.Courses.AnyAsync(c => c.Id == command.CourseId))
                    return (false, null, "Invalid Course ID.");

                if (command.Translations == null || !command.Translations.Any())
                    return (false, null, "At least one translation is required.");

                var validLanguages = new[] { "ar", "en", "ru" };
                foreach (var t in command.Translations)
                {
                    if (!validLanguages.Contains(t.Language))
                        return (false, null, $"Invalid language: {t.Language}. Must be 'ar', 'en', or 'ru'.");
                    if (string.IsNullOrWhiteSpace(t.Name) || t.Name.Length > 255)
                        return (false, null, "Name is required and must be 255 characters or less.");
                }

                var unit = new Unit
                {
                    CourseId = command.CourseId,
                    Translations = command.Translations.Select(t => new UnitTranslation
                    {
                        Language = t.Language,
                        Name = t.Name
                    }).ToList()
                };

                _context.Units.Add(unit);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Unit {UnitId} created for Course {CourseId}", unit.Id, command.CourseId);
                return (true, unit, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating unit for CourseId {CourseId}", command.CourseId);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, UnitWithTranslationViewModel? Unit, string? ErrorMessage)> GetUnitWithTranslationAsync(GetUnitWithTranslationQuery command)
        {
            try
            {
                var validLanguages = new[] { "ar", "en", "ru" };
                if (!validLanguages.Contains(command.Language))
                    return (false, null, $"Invalid language: {command.Language}. Must be 'ar', 'en', or 'ru'.");

                var unit = await _context.Units
                    .Include(u => u.Translations)
                    .Include(u => u.Lessons)
                    .FirstOrDefaultAsync(u => u.Id == command.UnitId);

                if (unit == null)
                    return (false, null, "Unit not found.");

                var translation = unit.Translations.FirstOrDefault(t => t.Language == command.Language);
                if (translation == null)
                    return (false, null, $"No translation found for language '{command.Language}'.");

                var viewModel = new UnitWithTranslationViewModel
                {
                    Id = unit.Id,
                    CourseId = unit.CourseId,
                    TranslationName = translation.Name,
                    LessonCount = unit.Lessons.Count
                };

                _logger.LogInformation("Retrieved unit {UnitId} with translation in {Language}", command.UnitId, command.Language);
                return (true, viewModel, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unit {UnitId} with translation in {Language}", command.UnitId, command.Language);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, List<UnitSummaryWithTranslationViewModel> Units, string? ErrorMessage)> GetAllUnitsForCourseAsync(GetAllUnitsForCourseQuery command)
        {
            try
            {
                var validLanguages = new[] { "ar", "en", "ru" };
                if (!validLanguages.Contains(command.Language))
                    return (false, null, $"Invalid language: {command.Language}. Must be 'ar', 'en', or 'ru'.");

                if (!await _context.Courses.AnyAsync(c => c.Id == command.CourseId))
                    return (false, null, "Invalid Course ID.");

                var units = await _context.Units
                    .Include(u => u.Translations)
                    .Include(u => u.Lessons)
                    .Where(u => u.CourseId == command.CourseId && u.Translations.Any(t => t.Language == command.Language))
                    .ToListAsync();

                var viewModels = units.Select(u =>
                {
                    var translation = u.Translations.First(t => t.Language == command.Language);
                    return new UnitSummaryWithTranslationViewModel
                    {
                        Id = u.Id,
                        TranslationName = translation.Name,
                        LessonCount = u.Lessons.Count
                    };
                }).ToList();

                _logger.LogInformation("Retrieved {UnitCount} units for Course {CourseId} with translations in {Language}", viewModels.Count, command.CourseId, command.Language);
                return (true, viewModels, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving units for CourseId {CourseId} with translation in {Language}", command.CourseId, command.Language);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateUnitAsync(UpdateUnitCommand command)
        {
            try
            {
                var unit = await _context.Units
                    .Include(u => u.Translations)
                    .FirstOrDefaultAsync(u => u.Id == command.UnitId);

                if (unit == null)
                    return (false, "Unit not found.");

                if (command.Translations == null || !command.Translations.Any())
                    return (false, "At least one translation is required.");

                var validLanguages = new[] { "ar", "en", "ru" };
                foreach (var t in command.Translations)
                {
                    if (!validLanguages.Contains(t.Language))
                        return (false, $"Invalid language: {t.Language}. Must be 'ar', 'en', or 'ru'.");
                    if (string.IsNullOrWhiteSpace(t.Name) || t.Name.Length > 255)
                        return (false, "Name is required and must be 255 characters or less.");
                }

                // Remove existing translations
                _context.UnitTranslations.RemoveRange(unit.Translations);
                unit.Translations.Clear();

                // Add new translations
                unit.Translations.AddRange(command.Translations.Select(t => new UnitTranslation
                {
                    UnitId = unit.Id,
                    Language = t.Language,
                    Name = t.Name
                }));

                await _context.SaveChangesAsync();

                _logger.LogInformation("Unit {UnitId} updated with {TranslationCount} translations", command.UnitId, command.Translations.Count);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating unit {UnitId}", command.UnitId);
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteUnitAsync(DeleteUnitCommand command)
        {
            try
            {
                var unit = await _context.Units
                    .Include(u => u.Translations)
                    .Include(u => u.Lessons)
                    .FirstOrDefaultAsync(u => u.Id == command.UnitId);

                if (unit == null)
                    return (false, "Unit not found.");

                // Cascade delete will handle Translations and Lessons (per your DBContext)
                _context.Units.Remove(unit);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Unit {UnitId} deleted", command.UnitId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting unit {UnitId}", command.UnitId);
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, Group? Group, string? ErrorMessage)> CreateGroupAsync(CreateGroupCommand command)
        {
            try
            {
                if (!await _context.Courses.AnyAsync(c => c.Id == command.CourseId))
                    return (false, null, "Invalid Course ID.");

                if (string.IsNullOrWhiteSpace(command.InstructorId) || !await _context.Users.AnyAsync(u => u.Id == command.InstructorId))
                    return (false, null, "Invalid Instructor ID.");

                if (command.MaxStudents <= 0)
                    return (false, null, "MaxStudents must be greater than zero.");

                if (command.Translations == null || !command.Translations.Any())
                    return (false, null, "At least one translation is required.");

                var validLanguages = new[] { "ar", "en", "ru" };
                foreach (var t in command.Translations)
                {
                    if (!validLanguages.Contains(t.Language))
                        return (false, null, $"Invalid language: {t.Language}. Must be 'ar', 'en', or 'ru'.");
                    if (string.IsNullOrWhiteSpace(t.Name))
                        return (false, null, "Name is required for each translation.");
                }

                var students = await _context.Users
                    .Where(u => command.StudentIds.Contains(u.Id))
                    .ToListAsync();
                if (students.Count != command.StudentIds.Count)
                {
                    var missingIds = command.StudentIds.Except(students.Select(s => s.Id)).ToList();
                    return (false, null, $"The following Student IDs do not exist: {string.Join(", ", missingIds)}");
                }
                if (students.Count > command.MaxStudents)
                    return (false, null, $"Student count ({students.Count}) exceeds MaxStudents ({command.MaxStudents}).");

                var group = new Group
                {
                    CourseId = command.CourseId,
                    InstructorId = command.InstructorId,
                    MaxStudents = command.MaxStudents,
                    Translations = command.Translations.Select(t => new GroupTranslation
                    {
                        Language = t.Language,
                        Name = t.Name,
                        Description = t.Description
                    }).ToList(),
                    GroupStudents = students.Select(s => new GroupStudent
                    {
                        StudentId = s.Id
                    }).ToList()
                };

                _context.Groups.Add(group);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Group {GroupId} created for Course {CourseId}", group.Id, command.CourseId);
                return (true, group, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group for CourseId {CourseId}", command.CourseId);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, GroupWithTranslationViewModel? Group, string? ErrorMessage)> GetGroupWithTranslationAsync(GetGroupWithTranslationQuery query)
        {
            try
            {
                var validLanguages = new[] { "ar", "en", "ru" };
                if (!validLanguages.Contains(query.Language))
                    return (false, null, $"Invalid language: {query.Language}. Must be 'ar', 'en', or 'ru'.");

                var group = await _context.Groups
                    .Include(g => g.Translations)
                    .Include(g => g.GroupStudents)
                    .FirstOrDefaultAsync(g => g.Id == query.GroupId);

                if (group == null)
                    return (false, null, "Group not found.");

                var translation = group.Translations.FirstOrDefault(t => t.Language == query.Language);
                if (translation == null)
                    return (false, null, $"No translation found for language '{query.Language}'.");

                var viewModel = new GroupWithTranslationViewModel
                {
                    Id = group.Id,
                    CourseId = group.CourseId,
                    InstructorId = group.InstructorId,
                    MaxStudents = group.MaxStudents,
                    CurrentStudentCount = group.GroupStudents.Count,
                    StudentIds = group.GroupStudents.Select(gs => gs.StudentId).ToList(),
                    TranslationName = translation.Name,
                    TranslationDescription = translation.Description
                };

                _logger.LogInformation("Retrieved group {GroupId} with translation in {Language}", query.GroupId, query.Language);
                return (true, viewModel, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group {GroupId} with translation in {Language}", query.GroupId, query.Language);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, List<GroupSummaryWithTranslationViewModel> Groups, string? ErrorMessage)> GetAllGroupsForCourseAsync(GetAllGroupsForCourseQuery query)
        {
            try
            {
                var validLanguages = new[] { "ar", "en", "ru" };
                if (!validLanguages.Contains(query.Language))
                    return (false, null, $"Invalid language: {query.Language}. Must be 'ar', 'en', or 'ru'.");

                if (!await _context.Courses.AnyAsync(c => c.Id == query.CourseId))
                    return (false, null, "Invalid Course ID.");

                var groups = await _context.Groups
                    .Include(g => g.Translations)
                    .Include(g => g.GroupStudents)
                    .Where(g => g.CourseId == query.CourseId && g.Translations.Any(t => t.Language == query.Language))
                    .ToListAsync();

                var viewModels = groups.Select(g =>
                {
                    var translation = g.Translations.First(t => t.Language == query.Language);
                    return new GroupSummaryWithTranslationViewModel
                    {
                        Id = g.Id,
                        InstructorId = g.InstructorId,
                        MaxStudents = g.MaxStudents,
                        CurrentStudentCount = g.GroupStudents.Count,
                        TranslationName = translation.Name
                    };
                }).ToList();

                _logger.LogInformation("Retrieved {GroupCount} groups for Course {CourseId} with translations in {Language}", viewModels.Count, query.CourseId, query.Language);
                return (true, viewModels, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving groups for CourseId {CourseId} with translation in {Language}", query.CourseId, query.Language);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateGroupAsync(UpdateGroupCommand command)
        {
            try
            {
                var group = await _context.Groups
                    .Include(g => g.Translations)
                    .Include(g => g.GroupStudents)
                    .FirstOrDefaultAsync(g => g.Id == command.GroupId);

                if (group == null)
                    return (false, "Group not found.");

                if (string.IsNullOrWhiteSpace(command.InstructorId) || !await _context.Users.AnyAsync(u => u.Id == command.InstructorId))
                    return (false, "Invalid Instructor ID.");

                if (command.MaxStudents <= 0)
                    return (false, "MaxStudents must be greater than zero.");

                if (command.MaxStudents < group.GroupStudents.Count)
                    return (false, $"MaxStudents ({command.MaxStudents}) cannot be less than current student count ({group.GroupStudents.Count}).");

                if (command.Translations == null || !command.Translations.Any())
                    return (false, "At least one translation is required.");

                var validLanguages = new[] { "ar", "en", "ru" };
                foreach (var t in command.Translations)
                {
                    if (!validLanguages.Contains(t.Language))
                        return (false, $"Invalid language: {t.Language}. Must be 'ar', 'en', or 'ru'.");
                    if (string.IsNullOrWhiteSpace(t.Name))
                        return (false, "Name is required for each translation.");
                }

                group.InstructorId = command.InstructorId;
                group.MaxStudents = command.MaxStudents;
                _context.GroupsTranslations.RemoveRange(group.Translations);
                group.Translations.Clear();
                group.Translations.AddRange(command.Translations.Select(t => new GroupTranslation
                {
                    GroupId = group.Id,
                    Language = t.Language,
                    Name = t.Name,
                    Description = t.Description
                }));

                await _context.SaveChangesAsync();

                _logger.LogInformation("Group {GroupId} updated with {TranslationCount} translations", command.GroupId, command.Translations.Count);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group {GroupId}", command.GroupId);
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteGroupAsync(DeleteGroupCommand command)
        {
            try
            {
                var group = await _context.Groups
                    .Include(g => g.Translations)
                    .Include(g => g.GroupStudents)
                    .FirstOrDefaultAsync(g => g.Id == command.GroupId);

                if (group == null)
                    return (false, "Group not found.");

                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Group {GroupId} deleted", command.GroupId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group {GroupId}", command.GroupId);
                return (false, $"An error occurred: {ex.Message}");
            }
        }
    }
}