using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LMS_API.Controllers.Courses.Commands;
using LMS_API.Services;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly CourseService _courseService;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(CourseService courseService, ILogger<CoursesController> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddCourse([FromBody] CreateCourseRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var course = await _courseService.AddCourseAsync(request);
        return Ok(course);
    }

    [HttpPost("{courseId}/translations")]
    public async Task<IActionResult> AddCourseTranslation(Guid courseId, [FromBody] AddTranslationRequest request)
    {
        var translation = await _courseService.AddCourseTranslationAsync(courseId, request.Language, request.Name, request.Description);
        return Ok(translation);
    }

    //[HttpPost("{courseId}/groups")]
    //public async Task<IActionResult> AddGroupToCourse(Guid courseId, [FromBody] AddGroupRequest request)
    //{
    //    var group = await _courseService.AddGroupToCourseAsync(courseId, request.InstructorId);
    //    return Ok(group);
    //}

    //[HttpPost("groups/{groupId}/translations")]
    //public async Task<IActionResult> AddGroupTranslation(Guid groupId, [FromBody] AddTranslationRequest request)
    //{
    //    var translation = await _courseService.AddGroupTranslationAsync(groupId, request.Language, request.Name, request.Description);
    //    return Ok(translation);
    //}

    //[HttpPost("groups/{groupId}/students")]
    //public async Task<IActionResult> AddStudentToGroup(Guid groupId, [FromBody] AddStudentRequest request)
    //{
    //    var groupStudent = await _courseService.AddStudentToGroupAsync(groupId, request.StudentId);
    //    return Ok(groupStudent);
    //}
}
