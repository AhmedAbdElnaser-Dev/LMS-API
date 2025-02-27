using LMS_API.Controllers.Courses.Commands;
using LMS_API.Controllers.Courses.ViewModels;
using LMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS_API.Controllers
{
    [Route("api/courses")]
    [ApiController]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly CourseService _courseService;

        public BooksController(CourseService courseService)
        {
            _courseService = courseService;
        }

        //[HttpPost]
        //[Authorize(Roles = "Teacher,Admin")]
        //public async Task<IActionResult> CreateCourse([FromBody] CreateCourseCommand command)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(userId))
        //        return Unauthorized();

        //    try
        //    {
        //        var course = await _courseService.CreateCourseAsync(
        //            Guid.Parse(userId),
        //            command
        //        );
        //        return Ok(course);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { Message = ex.Message });
        //    }
        //}
    }
}