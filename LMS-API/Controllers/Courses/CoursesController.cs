using LMS_API.Controllers.Courses.Commands;
using LMS_API.Controllers.Courses.Queries;
using LMS_API.Data;
using LMS_API.Helpers;
using LMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS_API.Controllers
{
    [ApiController]
    [Route("api/courses")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly CourseService _courseService;
        private readonly ILogger<CoursesController> _logger;
        private readonly DBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CoursesController(
            CourseService courseService,
            ILogger<CoursesController> logger,
            DBContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _courseService = courseService;
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCoursesDetailed()
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Courses"))
            //    return Forbid();

            var (success, courses, errorMessage) = await _courseService.GetAllCoursesDetailedAsync();
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(courses);
        }

        [HttpGet("{courseId}/full-details")]
        public async Task<IActionResult> GetCourseFullDetails(Guid courseId)
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Course"))
            //    return Forbid();

            var (success, course, errorMessage) = await _courseService.GetCourseFullDetailsAsync(courseId);
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(course);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddCourse([FromBody] AddCourseCommand command)
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Add_Course"))
            //    return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "Invalid or missing token" });

            var (success, course, errorMessage) = await _courseService.AddCourseAsync(userId, command);
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(new { Message = "Course added successfully", CourseId = course!.Id });
        }

        [HttpPut("{courseId}/edit")]
        public async Task<IActionResult> EditCourse(Guid courseId, [FromBody] EditCourseCommand command)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var (success, course, errorMessage) = await _courseService.EditCourseAsync(userId, courseId, command);
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(new { Message = "Course updated successfully", CourseId = course!.Id });
        }

        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourse(Guid courseId)
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Delete_Course"))
            //    return Forbid();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "Invalid or missing token" });

            var (success, errorMessage) = await _courseService.DeleteCourseAsync(userId, courseId);
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(new { Message = "Course deleted successfully" });
        }

        [HttpPut("update-category")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCourseCategoryCommand command)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Update_Course_Category"))
                return Forbid();

            var result = await _courseService.UpdateCourseCategoryAsync(command);
            if (!result)
                return NotFound("Course or category not found");

            return Ok("Category updated successfully");
        }

        [HttpPut("update-books")]
        public async Task<IActionResult> UpdateBooks([FromBody] UpdateCoursesBooksCommand command)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Update_Course_Books"))
                return Forbid();

            var result = await _courseService.UpdateCoursesBooksAsync(command);
            if (!result)
                return NotFound("Course or books not found");

            return Ok("Books updated successfully");
        }

        [HttpDelete("remove-book")]
        public async Task<IActionResult> RemoveBook([FromBody] RemoveBookFromCourseCommand command)
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Remove_Course_Book"))
            //    return Forbid();

            var result = await _courseService.RemoveBookFromCourseAsync(command);
            if (!result)
                return NotFound("Course or book not found in the course");

            return Ok("Book removed from course successfully");
        }

        [HttpPost("translations/add")]
        public async Task<IActionResult> AddCourseTranslation([FromBody] AddCourseTranslationCommand command)
        {
            var requiredPermissions = new[] { $"Translate_{command.Language}", $"Add_Course_Translate_{command.Language}" };
            //if (!await PermissionHelpers.HasAnyPermissionAsync(_context, _httpContextAccessor.HttpContext, requiredPermissions))
            //    return Forbid();

            var (success, translation, errorMessage) = await _courseService.AddCourseTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Translation added", TranslationId = translation!.Id });
        }

        [HttpPut("translations/edit")]
        public async Task<IActionResult> EditCourseTranslation([FromBody] EditCourseTranslationCommand command)
        {
            var requiredPermissions = new[] { $"Translate_{command.Language}", $"Update_Course_Translate_{command.Language}" };
            //if (!await PermissionHelpers.HasAnyPermissionAsync(_context, _httpContextAccessor.HttpContext, requiredPermissions))
            //    return Forbid();

            var (success, translation, errorMessage) = await _courseService.EditCourseTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Translation updated", TranslationId = translation!.Id });
        }

        [HttpGet("{courseId}/translation/{language}")]
        public async Task<IActionResult> GetCourseWithTranslation(Guid courseId, string language)
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Course"))
            //    return Forbid();

            var command = new GetCourseWithTranslationQuery { CourseId = courseId, Language = language };
            var (success, course, errorMessage) = await _courseService.GetCourseWithTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(course);
        }

        [HttpGet("all/translation/{language}")]
        public async Task<IActionResult> GetAllCoursesWithTranslation(string language)
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Courses"))
            //    return Forbid();

            var command = new GetAllCoursesWithTranslationQuery { Language = language };
            var (success, courses, errorMessage) = await _courseService.GetAllCoursesWithTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(courses);
        }

        //[HttpPost("groups/add-student")]
        //public async Task<IActionResult> AddStudentToGroup([FromBody] AddStudentToGroupCommand command)
        //{
        //    //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Manage_Group_Students"))
        //    //    return Forbid();

        //    //if (!ModelState.IsValid)
        //    //    return BadRequest(ModelState);

        //    var result = await _courseService.AddStudentToGroupAsync(command);
        //    if (!result.Success)
        //        return BadRequest("Failed to add student to group");

        //    return Ok("Student added to group successfully");
        //}
    }
}