using LMS_API.Controllers.Courses.Commands;
using LMS_API.Controllers.Courses.ViewModels;
using LMS_API.Data;
using LMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services
{
    public class CourseService
    {
        private readonly DBContext _context;

        public CourseService(DBContext context)
        {
            _context = context;
        }

        //public async Task<CourseVM> CreateCourseAsync(Guid userId, CreateCourseCommand command)
        //{
        //    var category = await _context.Categories
        //        .FirstOrDefaultAsync(c => c.Id == command.CategoryId);

        //    if (category == null)
        //        throw new Exception("Category not found");

        //    var course = new Course
        //    {
        //        AddedBy = userId,
        //        CategoryId = command.CategoryId
        //    };

        //    _context.Courses.Add(course);
        //    await _context.SaveChangesAsync();

        //    return MapToVM(course);
        //}
    }
}