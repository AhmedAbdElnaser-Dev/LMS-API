using LMS_API.Controllers.Groups.Commands;
using LMS_API.Controllers.Groups.Queries;
using LMS_API.Controllers.Groups.ViewModels;
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
    public class GroupService
    {
        private readonly DBContext _context;
        private readonly ILogger<GroupService> _logger;

        public GroupService(DBContext context, ILogger<GroupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string? ErrorMessage)> AddStudentToGroupAsync(AddStudentToGroupCommand command)
        {
            try
            {
                var group = await _context.Groups
                    .Include(g => g.GroupStudents)
                    .FirstOrDefaultAsync(g => g.Id == command.GroupId);
                
                if (group == null)
                {
                    _logger.LogWarning("Group {GroupId} not found", command.GroupId);
                    return (false, "Group not found");
                }

                var student = await _context.Users.FindAsync(command.StudentId);
                if (student == null)
                {
                    _logger.LogWarning("Student {StudentId} not found", command.StudentId);
                    return (false, "Student not found");
                }

                if (await _context.GroupsStudents
                    .AnyAsync(gs => gs.GroupId == command.GroupId && gs.StudentId == command.StudentId))
                {
                    _logger.LogWarning("Student {StudentId} already in group {GroupId}", command.StudentId, command.GroupId);
                    return (false, "Student already in group");
                }

                if (group.GroupStudents.Count >= group.MaxStudents)
                {
                    _logger.LogWarning("Group {GroupId} is full (max {MaxStudents} students)", command.GroupId, group.MaxStudents);
                    return (false, "Group is full");
                }

                _context.GroupsStudents.Add(new GroupStudent 
                { 
                    GroupId = command.GroupId, 
                    StudentId = command.StudentId 
                });
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("Student {StudentId} added to group {GroupId}", command.StudentId, command.GroupId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding student to group");
                return (false, "An error occurred while adding student to group");
            }
        }

        public async Task<(bool Success, GroupTranslationViewModel Translation, string? ErrorMessage)> AddGroupTranslationAsync(AddGroupTranslationCommand command)
        {
            try
            {
                var group = await _context.Groups
                    .FirstOrDefaultAsync(g => g.Id == command.GroupId);
                if (group == null)
                {
                    _logger.LogWarning("Group {GroupId} not found", command.GroupId);
                    return (false, null, "Group not found.");
                }

                if (!new[] { "ar", "en", "ru" }.Contains(command.Language))
                {
                    _logger.LogWarning("Invalid language {Language} for group {GroupId}", command.Language, command.GroupId);
                    return (false, null, "Language must be 'ar', 'en', or 'ru'.");
                }
                
                if (await _context.GroupsTranslations.AnyAsync(t => t.GroupId == command.GroupId && t.Language == command.Language))
                {
                    _logger.LogWarning("Translation for group {GroupId} in language {Language} already exists", command.GroupId, command.Language);
                    return (false, null, $"Translation for language '{command.Language}' already exists.");
                }

                var translation = new GroupTranslation
                {
                    Id = Guid.NewGuid(),
                    GroupId = command.GroupId,
                    Language = command.Language,
                    Name = command.Name,
                    Description = command.Description
                };

                await _context.GroupsTranslations.AddAsync(translation);
                await _context.SaveChangesAsync();

                var viewModel = new GroupTranslationViewModel
                {
                    Id = translation.Id,
                    GroupId = translation.GroupId,
                    Language = translation.Language,
                    Name = translation.Name,
                    Description = translation.Description
                };

                _logger.LogInformation("Added translation for group {GroupId} in language {Language}", command.GroupId, command.Language);
                return (true, viewModel, null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error adding translation for group {GroupId} in language {Language}", command.GroupId, command.Language);
                return (false, null, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding translation for group {GroupId} in language {Language}", command.GroupId, command.Language);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, GroupTranslationViewModel Translation, string? ErrorMessage)> UpdateGroupTranslationAsync(UpdateGroupTranslationCommand command)
        {
            try
            {
                var group = await _context.Groups
                    .FirstOrDefaultAsync(g => g.Id == command.GroupId);
                if (group == null)
                {
                    _logger.LogWarning("Group {GroupId} not found", command.GroupId);
                    return (false, null, "Group not found.");
                }

                // Validate language
                if (!new[] { "ar", "en", "ru" }.Contains(command.Language))
                {
                    _logger.LogWarning("Invalid language {Language} for group {GroupId}", command.Language, command.GroupId);
                    return (false, null, "Language must be 'ar', 'en', or 'ru'.");
                }

                // Find existing translation
                var translation = await _context.GroupsTranslations
                    .FirstOrDefaultAsync(t => t.GroupId == command.GroupId && t.Language == command.Language);
                if (translation == null)
                {
                    _logger.LogWarning("Translation for group {GroupId} in language {Language} not found", command.GroupId, command.Language);
                    return (false, null, $"Translation for language '{command.Language}' not found.");
                }

                // Update translation
                translation.Name = command.Name;
                translation.Description = command.Description;

                _context.GroupsTranslations.Update(translation);
                await _context.SaveChangesAsync();

                // Map to view model
                var viewModel = new GroupTranslationViewModel
                {
                    Id = translation.Id,
                    GroupId = translation.GroupId,
                    Language = translation.Language,
                    Name = translation.Name,
                    Description = translation.Description
                };

                _logger.LogInformation("Updated translation for group {GroupId} in language {Language}", command.GroupId, command.Language);
                return (true, viewModel, null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating translation for group {GroupId} in language {Language}", command.GroupId, command.Language);
                return (false, null, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating translation for group {GroupId} in language {Language}", command.GroupId, command.Language);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, GroupViewModel Group, string? ErrorMessage)> AddGroupToCourseAsync(AddGroupToCourseCommand command)
        {
            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == command.CourseId);
                if (course == null)
                {
                    _logger.LogWarning("Course {CourseId} not found", command.CourseId);
                    return (false, null, "Course not found.");
                }

                var instructor = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == command.InstructorId);
                if (instructor == null)
                {
                    _logger.LogWarning("Instructor {InstructorId} not found", command.InstructorId);
                    return (false, null, "Instructor not found.");
                }

                var instructorRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "Instructor");
                if (instructorRole == null || !await _context.UserRoles.AnyAsync(ur => ur.UserId == command.InstructorId && ur.RoleId == instructorRole.Id))
                {
                    _logger.LogWarning("User {InstructorId} is not an instructor", command.InstructorId);
                    return (false, null, "User is not an instructor.");
                }

                if (await _context.Groups.AnyAsync(g => g.CourseId == command.CourseId && g.Name == command.Name))
                {
                    _logger.LogWarning("Group name {GroupName} already exists in course {CourseId}", command.Name, command.CourseId);
                    return (false, null, $"Group name '{command.Name}' already exists in this course.");
                }

                if (command.MaxStudents <= 0)
                {
                    _logger.LogWarning("Invalid MaxStudents {MaxStudents} for group in course {CourseId}", command.MaxStudents, command.CourseId);
                    return (false, null, "Maximum students must be greater than zero.");
                }

                var group = new Group
                {
                    Id = Guid.NewGuid(),
                    CourseId = command.CourseId,
                    Name = command.Name,
                    InstructorId = command.InstructorId,
                    MaxStudents = command.MaxStudents,
                    GroupStudents = new List<GroupStudent>(),
                    Translations = new List<GroupTranslation>()
                };

                await _context.Groups.AddAsync(group);
                await _context.SaveChangesAsync();

                var viewModel = new GroupViewModel
                {
                    Id = group.Id,
                    CourseId = group.CourseId.ToString(),
                    Name = group.Name,
                    InstructorId = group.InstructorId.ToString(),
                    MaxStudents = group.MaxStudents
                };

                _logger.LogInformation("Added group {GroupId} to course {CourseId}", group.Id, command.CourseId);
                return (true, viewModel, null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error adding group to course {CourseId}", command.CourseId);
                return (false, null, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding group to course {CourseId}", command.CourseId);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }


        public async Task<(bool Success, List<GroupViewModel> Groups, string ErrorMessage)> GetCourseGroupsAsync(Guid courseId)
        {
            try
            {
                var groups = await _context.Groups
                    .Where(g => g.CourseId == courseId)
                    .Include(g => g.Translations)
                    .Include(g => g.GroupStudents)
                    .Include(g => g.Course)
                    .Include(g => g.Instructor)
                    .Select(g => new GroupViewModel
                    {
                        Id = g.Id,
                        Name = g.Name,
                        MaxStudents = g.MaxStudents,
                        CurrentStudents = g.GroupStudents.Count,
                        CourseId = g.CourseId.ToString(),
                        InstructorId = g.InstructorId.ToString(),
                        Translations = g.Translations.Select(t => new GroupTranslationViewModel
                        {
                            Id = t.Id,
                            GroupId = t.GroupId,
                            Language = t.Language,
                            Name = t.Name,
                            Description = t.Description
                        }).ToList()
                    }).ToListAsync();

                return (true, groups, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course groups");
                return (false, null, "Error retrieving groups");
            }
        }

        public async Task<(bool Success, GroupDetailsViewModel Group, string? ErrorMessage)> GetGroupDetailsAsync(Guid groupId)
        {
            try
            {
                var group = await _context.Groups
                    .Include(g => g.Course)
                    .Include(g => g.Instructor)
                    .Include(g => g.GroupStudents).ThenInclude(gs => gs.Student)
                    .Include(g => g.Translations)
                    .FirstOrDefaultAsync(g => g.Id == groupId);

                if (group == null)
                {
                    _logger.LogWarning("Group {GroupId} not found", groupId);
                    return (false, null, "Group not found.");
                }

                // Map to view model
                var viewModel = new GroupDetailsViewModel
                {
                    Id = group.Id,
                    Name = group.Name,
                    MaxStudents = group.MaxStudents,
                    CourseId = group.CourseId,
                    Course = new CourseViewModel
                    {
                        Id = group.Course.Id,
                        Name = group.Course.Name
                    },
                    InstructorId = group.InstructorId,
                    Instructor = new InstructorViewModel
                    {
                        Id = group.Instructor.Id,
                        FullName = $"{group.Instructor.FirstName} {group.Instructor.LastName}".Trim(),
                        Email = group.Instructor.Email
                    },
                    Students = group.GroupStudents.Select(gs => new StudentViewModel
                    {
                        Id = gs.StudentId,
                        FullName = $"{gs.Student.FirstName} {gs.Student.LastName}".Trim(),
                        Email = gs.Student.Email
                    }).ToList(),
                    Translations = group.Translations.Select(t => new GroupTranslationViewModel
                    {
                        Id = t.Id,
                        GroupId = t.GroupId,
                        Language = t.Language,
                        Name = t.Name,
                        Description = t.Description
                    }).ToList()
                };

                _logger.LogInformation("Retrieved details for group {GroupId}", groupId);
                return (true, viewModel, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving details for group {GroupId}", groupId);
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }
        public async Task<(bool Success, Group Group, string ErrorMessage)> EditGroupAsync(EditGroupCommand command)
        {
            try
            {
                var group = await _context.Groups.FindAsync(command.GroupId);
                if (group == null)
                    return (false, null, "Group not found");

                group.Name = command.Name;
                group.MaxStudents = command.MaxStudents;
                
                await _context.SaveChangesAsync();
                return (true, group, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing group");
                return (false, null, "Error updating group");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> RemoveStudentFromGroupAsync(RemoveStudentFromGroupCommand command)
        {
            try
            {
                var groupStudent = await _context.GroupsStudents
                    .FirstOrDefaultAsync(gs => gs.GroupId == command.GroupId && gs.StudentId == command.StudentId);

                if (groupStudent == null)
                    return (false, "Student not found in group");

                _context.GroupsStudents.Remove(groupStudent);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing student from group");
                return (false, "Error removing student");
            }
        }

        public async Task<(bool Success, List<GroupStudentViewModel> Students, string ErrorMessage)> GetGroupStudentsAsync(Guid groupId)
        {
            try
            {
                var students = await _context.GroupsStudents
                    .Where(gs => gs.GroupId == groupId)
                    .Include(gs => gs.Student)
                    .Select(gs => new GroupStudentViewModel
                    {
                        StudentId = gs.StudentId,
                        FullName = gs.Student.FirstName + gs.Student.LastName,
                        Email = gs.Student.Email
                    })
                    .ToListAsync();

                return (true, students, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group students");
                return (false, null, "Error retrieving students");
            }
        }
    }
}
