using LMS_API.Controllers.Courses.Commands;
using LMS_API.Data;
using LMS_API.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Course> AddCourseAsync(CreateCourseRequest request)
        {
            var course = new Course
            {
                Id = Guid.NewGuid(),
                Books = await _context.Books.Where(b => request.BookIds.Contains(b.Id)).ToListAsync(),
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<CourseTranslation> AddCourseTranslationAsync(Guid courseId, string language, string name, string description)
        {
            var translation = new CourseTranslation
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                Language = language,
                Name = name,
                Description = description
            };

            _context.CoursesTranslations.Add(translation);
            await _context.SaveChangesAsync();
            return translation;
        }

        //public async Task<Group> AddGroupToCourseAsync(Guid courseId, string instructorId)
        //{
        //    var group = new Group
        //    {
        //        Id = Guid.NewGuid(),
        //        CourseId = courseId,
        //        InstructorId = instructorId
        //    };

        //    _context.Groups.Add(group);
        //    await _context.SaveChangesAsync();
        //    return group;
        //}

        //public async Task<GroupTranslation> AddGroupTranslationAsync(Guid groupId, string language, string name, string description)
        //{
        //    var translation = new GroupTranslation
        //    {
        //        Id = Guid.NewGuid(),
        //        GroupId = groupId,
        //        Language = language,
        //        Name = name,
        //        Description = description
        //    };

        //    _context.GroupsTranslations.Add(translation);
        //    await _context.SaveChangesAsync();
        //    return translation;
        //}

        //public async Task<GroupStudent> AddStudentToGroupAsync(Guid groupId, Guid studentId)
        //{
        //    var groupStudent = new GroupStudent
        //    {
        //        Id = Guid.NewGuid(),
        //        GroupId = groupId,
        //        StudentId = studentId
        //    };

        //    _context.GroupsStudents.Add(groupStudent);
        //    await _context.SaveChangesAsync();
        //    return groupStudent;
        //}
    }
}