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