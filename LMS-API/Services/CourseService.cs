using LMS_API.Controllers.Courses.Commands;
using LMS_API.Controllers.Courses.Queries;
using LMS_API.Controllers.Courses.ViewModels;
using LMS_API.Data;
using LMS_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Errors.Model;
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

        public async Task<(bool Success, List<CourseDetailsViewModel> Courses, string? ErrorMessage)> GetAllCoursesDetailedAsync()
        {
            try
            {
                var courses = await _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.Department)
                    .Include(c => c.Units)
                        .ThenInclude(u => u.Lessons)
                    .Include(c => c.Groups)
                        .ThenInclude(g => g.Instructor)
                    .ToListAsync();

                var viewModels = courses.Select(c =>
                {
                    var instructors = c.Groups
                        .Select(g => new UserViewModel
                        {
                            Id = g.InstructorId,
                            FullName = g.Instructor != null ? $"{g.Instructor.FirstName} {g.Instructor.LastName}" : "Unknown Instructor"
                        })
                        .DistinctBy(i => i.Id) 
                        .ToList();

                    return new CourseDetailsViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CategoryId = c.CategoryId,
                        CategoryName = c.Category?.Name ?? "Unknown",
                        DepartmentId = c.DepartmentId,
                        DepartmentName = c.Department?.Name ?? "Unknown", 
                        GroupCount = c.Groups.Count,
                        UnitCount = c.Units.Count,
                        LessonCount = c.Units.Sum(u => u.Lessons.Count),
                        InstructorCount = instructors.Count, 
                        Instructors = instructors 
                    };
                }).ToList();

                _logger.LogInformation("Retrieved {CourseCount} courses with detailed info", viewModels.Count);
                return (true, viewModels, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving detailed courses");
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, CourseFullDetailsViewModel? Course, string? ErrorMessage)> GetCourseFullDetailsAsync(Guid courseId)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.Department)
                    .Include(c => c.Groups)
                        .ThenInclude(g => g.Instructor)
                    .Include(c => c.CourseBooks)
                        .ThenInclude(cb => cb.Book)
                    .Include(c => c.Units)
                        .ThenInclude(u => u.Lessons)
                    .Include(c => c.Translations)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                    return (false, null, "Course not found.");

                var viewModel = new CourseFullDetailsViewModel
                {
                    Id = course.Id,
                    Name = course.Name,
                    CategoryId = course.CategoryId,
                    CategoryName = course.Category?.Name ?? "Unknown",
                    DepartmentId = course.DepartmentId,
                    DepartmentName = course.Department?.Name ?? "Unknown",
                    Groups = course.Groups.Select(g => new GroupInfo
                    {
                        Id = g.Id,
                        Instructor = new UserViewModel
                        {
                            Id = g.InstructorId,
                            FullName = g.Instructor != null ? $"{g.Instructor.FirstName} {g.Instructor.LastName}" : "Unknown Instructor"
                        }
                    }).ToList(),
                    Books = course.CourseBooks.Select(cb => new BookInfo
                    {
                        Id = cb.Book.Id,
                        Name = cb.Book.Name,
                        urlPdf = cb.Book.UrlPdf,
                        urlPic = cb.Book.UrlPic
                    }).ToList(),
                    Units = course.Units.Select(u => new UnitInfo
                    {
                        Id = u.Id,
                        Name = u.Translations.FirstOrDefault()?.Name ?? "Unnamed Unit",
                        LessonCount = u.Lessons.Count
                    }).ToList(),
                    Translations = new Dictionary<string, CourseTranslationInfo>
                        {
                            { "en", GetTranslationOrEmpty(course.Translations, "en") },
                            { "ar", GetTranslationOrEmpty(course.Translations, "ar") },
                            { "ru", GetTranslationOrEmpty(course.Translations, "ru") }
                        }
                };

                _logger.LogInformation("Retrieved full details for course {CourseId}", courseId);
                return (true, viewModel, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving full details for course {CourseId}", courseId);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        private CourseTranslationInfo GetTranslationOrEmpty(List<CourseTranslation> translations, string language)
        {
            var translation = translations.FirstOrDefault(t => t.Language == language);
            return translation != null
                ? new CourseTranslationInfo
                {
                    Id = translation.Id,
                    Name = translation.Name,
                    UrlPic = translation.UrlPic,
                    Description = translation.Description,
                    About = translation.About,
                    DemoUrl = translation.DemoUrl,
                    Title = translation.Title,
                    Prerequisites = translation.Prerequisites,
                    LearningOutcomes = translation.LearningOutcomes // Use deserialized List<string>
                }
                : new CourseTranslationInfo
                {
                    Id = Guid.Empty,
                    Name = null,
                    UrlPic = null,
                    Description = null,
                    About = null,
                    DemoUrl = null,
                    Title = null,
                    Prerequisites = new List<string>(), // Empty list for missing translation
                    LearningOutcomes = new List<string>() // Empty list for missing translation
                };
        }
        
        public async Task<(bool Success, Course? Course, string? ErrorMessage)> AddCourseAsync(string addedByUserId, AddCourseCommand command)
        {
            try
            {
                if (string.IsNullOrEmpty(addedByUserId))
                    return (false, null, "User ID is required.");

                if (string.IsNullOrWhiteSpace(command.Name))
                    return (false, null, "Course name is required.");

                if (await _context.Courses.AnyAsync(c => c.Name == command.Name))
                    return (false, null, $"A course with the name '{command.Name}' already exists.");

                if (!await _context.Categories.AnyAsync(c => c.Id == command.CategoryId))
                    return (false, null, "Invalid Category ID.");

                if (!await _context.Departments.AnyAsync(d => d.Id == command.DepartmentId))
                    return (false, null, "Invalid Department ID.");

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
                    Name = command.Name,          
                    CategoryId = command.CategoryId, 
                    DepartmentId = command.DepartmentId 
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                var coursesBooks = books.Select(b => new CourseBook
                {
                    CourseId = course.Id,
                    BookId = b.Id
                }).ToList();

                _context.CoursesBooks.AddRange(coursesBooks);
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
        
        public async Task<(bool Success, Course? Course, string? ErrorMessage)> EditCourseAsync(string editedByUserId, Guid courseId, EditCourseCommand command)
        {
            try
            {
                if (string.IsNullOrEmpty(editedByUserId))
                    return (false, null, "User ID is required.");

                var course = await _context.Courses
                    .Include(c => c.CourseBooks)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                    return (false, null, "Course not found.");

                if (string.IsNullOrWhiteSpace(command.Name))
                    return (false, null, "Course name is required.");

                if (await _context.Courses.AnyAsync(c => c.Name == command.Name && c.Id != courseId))
                    return (false, null, $"A course with the name '{command.Name}' already exists.");

                if (!await _context.Categories.AnyAsync(c => c.Id == command.CategoryId))
                    return (false, null, "Invalid Category ID.");

                if (!await _context.Departments.AnyAsync(d => d.Id == command.DepartmentId))
                    return (false, null, "Invalid Department ID.");

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

                // Update course properties
                course.Name = command.Name;
                course.CategoryId = command.CategoryId;
                course.DepartmentId = command.DepartmentId;

                // Remove existing CourseBooks
                _context.CoursesBooks.RemoveRange(course.CourseBooks);
                await _context.SaveChangesAsync();

                // Add new CourseBooks
                var newCourseBooks = books.Select(b => new CourseBook
                {
                    CourseId = course.Id,
                    BookId = b.Id
                }).ToList();

                _context.CoursesBooks.AddRange(newCourseBooks);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Course {CourseId} edited by user {UserId} with {BookCount} books", course.Id, editedByUserId, books.Count);
                return (true, course, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing course {CourseId} by user {UserId}", courseId, editedByUserId);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteCourseAsync(string deletedByUserId, Guid courseId)
        {
            try
            {
                if (string.IsNullOrEmpty(deletedByUserId))
                    return (false, "User ID is required.");

                var course = await _context.Courses
                    .Include(c => c.CourseBooks) 
                    .Include(c => c.Groups)
                    .Include(c => c.Units)
                        .ThenInclude(u => u.Lessons) 
                    .Include(c => c.Translations)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                    return (false, "Course not found.");

                //_context.CoursesBooks.RemoveRange(course.CourseBooks);
                //_context.Groups.RemoveRange(course.Groups);
                //_context.Units.RemoveRange(course.Units);
                //_context.CoursesTranslations.RemoveRange(course.Translations);

                _context.Courses.Remove(course);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Course {CourseId} deleted by user {UserId}", courseId, deletedByUserId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course {CourseId} by user {UserId}", courseId, deletedByUserId);
                return (false, $"An error occurred: {ex.Message}");
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

                var group = new Group
                {
                    CourseId = command.CourseId,
                    Name = command.Name,
                    InstructorId = command.InstructorId,
                    MaxStudents = command.MaxStudents
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

        public async Task<(bool Success, List<GroupSummaryViewModel> Groups, string? ErrorMessage)> GetCourseGroupsAsync(Guid courseId)
        {
            try
            {
                if (!await _context.Courses.AnyAsync(c => c.Id == courseId))
                    return (false, null, "Invalid Course ID.");

                var groups = await _context.Groups
                    .Include(g => g.GroupStudents)
                    .Where(g => g.CourseId == courseId)
                    .ToListAsync();

                var viewModels = groups.Select(g => new GroupSummaryViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    InstructorId = g.InstructorId,
                    MaxStudents = g.MaxStudents,
                    CurrentStudentCount = g.GroupStudents.Count
                }).ToList();

                _logger.LogInformation("Retrieved {GroupCount} groups for Course {CourseId}", viewModels.Count, courseId);
                return (true, viewModels, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving groups for CourseId {CourseId}", courseId);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, GroupDetailsWithTranslationsViewModel? Group, string? ErrorMessage)> GetGroupDetailsWithTranslationsAsync(Guid groupId)
        {
            try
            {
                var group = await _context.Groups
                    .Include(g => g.Translations)
                    .Include(g => g.GroupStudents)
                        .ThenInclude(gs => gs.Student)
                    .Include(g => g.Instructor) 
                    .FirstOrDefaultAsync(g => g.Id == groupId);

                if (group == null)
                    return (false, null, "Group not found.");

                var viewModel = new GroupDetailsWithTranslationsViewModel
                {
                    Id = group.Id,
                    Name = group.Name,
                    CourseId = group.CourseId,
                    MaxStudents = group.MaxStudents,
                    CurrentStudentCount = group.GroupStudents.Count,
                    Instructor = new UserViewModel
                    {
                        Id = group.Instructor.Id,
                        FullName = group.Instructor.FirstName + group.Instructor.LastName,
                        PhoneNumber = group.Instructor.PhoneNumber,
                        Email = group.Instructor.Email
                    },
                    Students = group.GroupStudents.Select(gs => new UserViewModel
                    {
                        Id = gs.Student.Id,
                        FullName = gs.Student.FirstName + gs.Student.LastName,
                        PhoneNumber = gs.Student.TelegramNumber,
                        Email = gs.Student.Email
                    }).ToList(),
                    Translations = group.Translations.Select(t => new GroupTranslationViewModel
                    {
                        Language = t.Language,
                        Name = t.Name,
                        Description = t.Description
                    }).ToList()
                };

                _logger.LogInformation("Retrieved group {GroupId} with {TranslationCount} translations, {StudentCount} students, and instructor",
                    groupId, viewModel.Translations.Count, viewModel.Students.Count);
                return (true, viewModel, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group {GroupId} details", groupId);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, Guid TranslationId, string? ErrorMessage)> AddGroupTranslationAsync(CreateGroupTranslationCommand command)
        {
            try
            {
                var group = await _context.Groups
                    .Include(g => g.Translations)
                    .FirstOrDefaultAsync(g => g.Id == command.GroupId);
                if (group == null)
                    return (false, Guid.Empty, "Group not found.");

                var validLanguages = new[] { "ar", "en", "ru" };
                if (!validLanguages.Contains(command.Language))
                    return (false, Guid.Empty, $"Invalid language: {command.Language}. Must be 'ar', 'en', or 'ru'.");

                if (group.Translations.Any(t => t.Language == command.Language))
                    return (false, Guid.Empty, $"Translation for language '{command.Language}' already exists.");

                if (string.IsNullOrWhiteSpace(command.Name))
                    return (false, Guid.Empty, "Translation name is required.");

                var translation = new GroupTranslation
                {
                    Id = Guid.NewGuid(), 
                    GroupId = command.GroupId,
                    Language = command.Language,
                    Name = command.Name,
                    Description = command.Description
                };

                _context.GroupsTranslations.Add(translation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added translation {TranslationId} for group {GroupId} in language {Language}",
                    translation.Id, command.GroupId, command.Language);
                return (true, translation.Id, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding translation for group {GroupId} in language {Language}",
                    command.GroupId, command.Language);
                return (false, Guid.Empty, $"An error occurred: {ex.Message}");
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


                group.Name = command.Name;
                group.InstructorId = command.InstructorId;
                group.MaxStudents = command.MaxStudents;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Group {GroupId} updated successfully", command.GroupId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group {GroupId}", command.GroupId);
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateGroupTranslationAsync(UpdateGroupTranslationCommand command)
        {
            try
            {
                // Validate group exists
                var group = await _context.Groups
                    .FirstOrDefaultAsync(g => g.Id == command.GroupId);
                if (group == null)
                {
                    _logger.LogWarning("Group {GroupId} not found", command.GroupId);
                    return (false, "Group not found.");
                }

                // Validate translation exists
                var translation = await _context.GroupsTranslations
                    .FirstOrDefaultAsync(t => t.GroupId == command.GroupId && t.Language == command.Language);
                if (translation == null)
                {
                    _logger.LogWarning("Translation for group {GroupId} and language {Language} not found", command.GroupId, command.Language);
                    return (false, $"Translation for language '{command.Language}' not found.");
                }

                // Update translation
                translation.Name = command.Name;
                translation.Description = command.Description ?? translation.Description; // Preserve existing if null
                _context.GroupsTranslations.Update(translation);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated translation for group {GroupId} in language {Language}", command.GroupId, command.Language);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating translation for group {GroupId} in language {Language}", command.GroupId, command.Language);
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

        public async Task<CourseTranslation> GetCourseTranslationById(Guid translationId)
        {
            try
            {
                var translation = await _context.CoursesTranslations
                                    .Include(ct => ct.Course)
                                    .FirstOrDefaultAsync(ct => ct.Id == translationId);
                if (translation == null)
                {
                    _logger.LogWarning("Course translation {TranslationId} not found", translationId);
                    throw new Exception("Course translation not found");
                }
                return translation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course translation {TranslationId}", translationId);
                throw; 
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> AddStudentToGroupAsync(AddStudentToGroupCommand command)
        {
            try
            {
                // Validate group
                var group = await _context.Groups
                    .Include(g => g.GroupStudents)
                    .FirstOrDefaultAsync(g => g.Id == command.GroupId);
                if (group == null)
                {
                    _logger.LogWarning("Attempted to add student to non-existent group {GroupId}", command.GroupId);
                    return (false, "Group does not exist.");
                }

                // Validate student
                var student = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == command.StudentId);
                if (student == null)
                {
                    _logger.LogWarning("Student {StudentId} not found for group {GroupId}", command.StudentId, command.GroupId);
                    return (false, "Student does not exist.");
                }

                //if (!await _context.UserRoles.AnyAsync(ur => ur.UserId == command.StudentId && ur.RoleId == (await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student"))?.Id))
                //{
                //    _logger.LogWarning("User {StudentId} is not a student for group {GroupId}", command.StudentId, command.GroupId);
                //    return (false, "User is not registered as a student.");
                //}

                // Check duplicate
                if (group.GroupStudents.Any(gs => gs.StudentId == command.StudentId))
                {
                    _logger.LogInformation("Student {StudentId} already in group {GroupId}", command.StudentId, command.GroupId);
                    return (false, "Student is already in the group.");
                }

                // Check max students
                if (group.GroupStudents.Count >= group.MaxStudents)
                {
                    _logger.LogWarning("Group {GroupId} at max capacity {MaxStudents}", command.GroupId, group.MaxStudents);
                    return (false, $"Group has reached its maximum capacity of {group.MaxStudents} students.");
                }

                // Add student
                var groupStudent = new GroupStudent
                {
                    GroupId = command.GroupId,
                    StudentId = command.StudentId
                };

                await _context.GroupsStudents.AddAsync(groupStudent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully added student {StudentId} to group {GroupId}", command.StudentId, command.GroupId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add student {StudentId} to group {GroupId}", command.StudentId, command.GroupId);
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> RemoveStudentFromGroupAsync(RemoveStudentFromGroupCommand command)
        {
            try
            {
                // Validate group
                var group = await _context.Groups
                    .Include(g => g.GroupStudents)
                    .FirstOrDefaultAsync(g => g.Id == command.GroupId);
                if (group == null)
                {
                    _logger.LogWarning("Attempted to remove student from non-existent group {GroupId}", command.GroupId);
                    return (false, "Group does not exist.");
                }

                // Validate student exists
                var student = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == command.StudentId);
                if (student == null)
                {
                    _logger.LogWarning("Student {StudentId} not found for group {GroupId}", command.StudentId, command.GroupId);
                    return (false, "Student does not exist.");
                }

                // Check if student is in group
                var groupStudent = group.GroupStudents
                    .FirstOrDefault(gs => gs.StudentId == command.StudentId);
                if (groupStudent == null)
                {
                    _logger.LogInformation("Student {StudentId} not found in group {GroupId}", command.StudentId, command.GroupId);
                    return (false, "Student is not in the group.");
                }

                // Remove student
                _context.GroupsStudents.Remove(groupStudent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully removed student {StudentId} from group {GroupId}", command.StudentId, command.GroupId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove student {StudentId} from group {GroupId}", command.StudentId, command.GroupId);
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, List<TeacherViewModel> Teachers, string? ErrorMessage)> GetTeachersAsync()
        {
            try
            {
                var teacherRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "Teacher");
                if (teacherRole == null)
                {
                    _logger.LogWarning("Teacher role not found");
                    return (false, new List<TeacherViewModel>(), "Teacher role not found.");
                }

                var teachers = await _context.Users
                    .Join(_context.UserRoles,
                        user => user.Id,
                        userRole => userRole.UserId,
                        (user, userRole) => new { User = user, UserRole = userRole })
                    .Where(x => x.UserRole.RoleId == teacherRole.Id)
                    .Select(x => new TeacherViewModel
                    {
                        Id = x.User.Id,
                        FullName = $"{x.User.FirstName} {x.User.LastName}".Trim(),
                        Email = x.User.Email,
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {TeacherCount} teachers", teachers.Count);
                return (true, teachers, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teachers");
                return (false, new List<TeacherViewModel>(), $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, List<StudentViewModel> Students, string? ErrorMessage)> GetStudentsAsync()
        {
            try
            {
                // Find Student role
                var studentRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "Student");
                if (studentRole == null)
                {
                    _logger.LogWarning("Student role not found");
                    return (false, new List<StudentViewModel>(), "Student role not found.");
                }

                // Get users with Student role
                var students = await _context.Users
                    .Join(_context.UserRoles,
                        user => user.Id,
                        userRole => userRole.UserId,
                        (user, userRole) => new { User = user, UserRole = userRole })
                    .Where(x => x.UserRole.RoleId == studentRole.Id)
                    .Select(x => new StudentViewModel
                    {
                        Id = x.User.Id,
                        FullName = $"{x.User.FirstName} {x.User.LastName}".Trim(),
                        Email = x.User.Email,
                        PhoneNumber = x.User.PhoneNumber,
                        Gender = x.User.Gender.ToString() 
                    })
                    .ToListAsync();

                return (true, students, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students");
                return (false, new List<StudentViewModel>(), $"An error occurred: {ex.Message}");
            }
        }
    }
}