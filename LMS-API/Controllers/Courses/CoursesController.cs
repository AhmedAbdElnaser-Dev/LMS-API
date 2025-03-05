using LMS_API.Controllers.Courses.Commands;
using LMS_API.Controllers.Courses.Queries;
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
    public class CoursesController : ControllerBase
    {
        private readonly CourseService _courseService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(CourseService courseService, ILogger<CoursesController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        [HttpPost("add")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> AddCourse([FromBody] AddCourseCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "Invalid or missing token" });

            var (success, course, errorMessage) = await _courseService.AddCourseAsync(userId, command);

            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(new
            {
                Message = "Course added successfully",
                CourseId = course!.Id,
            });
        }

        [HttpPut("update-category")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCourseCategoryCommand command)
        {
            var result = await _courseService.UpdateCourseCategoryAsync(command);
            if (!result)
                return NotFound("Course or category not found");

            return Ok("Category updated successfully");
        }

        [HttpPut("update-books")]
        public async Task<IActionResult> UpdateBooks([FromBody] UpdateCoursesBooksCommand command)
        {
            var result = await _courseService.UpdateCoursesBooksAsync(command);
            if (!result)
                return NotFound("Course or books not found");

            return Ok("Books updated successfully");
        }

        [HttpDelete("remove-book")]
        public async Task<IActionResult> RemoveBook([FromBody] RemoveBookFromCourseCommand command)
        {
            var result = await _courseService.RemoveBookFromCourseAsync(command);
            if (!result)
                return NotFound("Course or book not found in the course");

            return Ok("Book removed from course successfully");
        }

        [HttpPost("translations/add")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> AddCourseTranslation([FromBody] AddCourseTranslationCommand command)
        {
            var (success, translation, errorMessage) = await _courseService.AddCourseTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Translation added", TranslationId = translation!.Id });
        }

        [HttpPut("translations/edit")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> EditCourseTranslation([FromBody] EditCourseTranslationCommand command)
        {
            var (success, translation, errorMessage) = await _courseService.EditCourseTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Translation updated", TranslationId = translation!.Id });
        }

        [HttpDelete("translations/delete")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> DeleteCourseTranslation([FromBody] DeleteCourseTranslationCommand command)
        {
            var success = await _courseService.DeleteCourseTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = "Translation not found or could not be deleted" });
            return Ok(new { Message = "Translation deleted" });
        }

        [HttpGet("{courseId}/translation/{language}")]
        public async Task<IActionResult> GetCourseWithTranslation(Guid courseId, string language)
        {
            var command = new GetCourseWithTranslationQuery { CourseId = courseId, Language = language };
            var (success, course, errorMessage) = await _courseService.GetCourseWithTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(course);
        }

        [HttpGet("all/translation/{language}")]
        public async Task<IActionResult> GetAllCoursesWithTranslation(string language)
        {
            var command = new GetAllCoursesWithTranslationQuery { Language = language };
            var (success, courses, errorMessage) = await _courseService.GetAllCoursesWithTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(courses);
        }

        [HttpPost("units/create")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> CreateUnit([FromBody] CreateUnitCommand command)
        {
            var (success, unit, errorMessage) = await _courseService.CreateUnitAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Unit created", UnitId = unit!.Id });
        }

        [HttpGet("units/{unitId}/translation/{language}")]
        public async Task<IActionResult> GetUnitWithTranslation(Guid unitId, string language)
        {
            var command = new GetUnitWithTranslationQuery { UnitId = unitId, Language = language };
            var (success, unit, errorMessage) = await _courseService.GetUnitWithTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(unit);
        }

        [HttpGet("courses/{courseId}/units/translation/{language}")]
        public async Task<IActionResult> GetAllUnitsForCourse(Guid courseId, string language)
        {
            var command = new GetAllUnitsForCourseQuery { CourseId = courseId, Language = language };
            var (success, units, errorMessage) = await _courseService.GetAllUnitsForCourseAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(units);
        }

        [HttpPut("units/update")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> UpdateUnit([FromBody] UpdateUnitCommand command)
        {
            var (success, errorMessage) = await _courseService.UpdateUnitAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Unit updated" });
        }

        [HttpDelete("units/delete")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> DeleteUnit([FromBody] DeleteUnitCommand command)
        {
            var (success, errorMessage) = await _courseService.DeleteUnitAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Unit deleted" });
        }

        [HttpPost("groups/create")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupCommand command)
        {
            var (success, group, errorMessage) = await _courseService.CreateGroupAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Group created", GroupId = group!.Id });
        }

        [HttpGet("groups/{groupId}/translation/{language}")]
        public async Task<IActionResult> GetGroupWithTranslation(Guid groupId, string language)
        {
            var query = new GetGroupWithTranslationQuery { GroupId = groupId, Language = language };
            var (success, group, errorMessage) = await _courseService.GetGroupWithTranslationAsync(query);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(group);
        }

        [HttpGet("courses/{courseId}/groups/translation/{language}")]
        public async Task<IActionResult> GetAllGroupsForCourse(Guid courseId, string language)
        {
            var query = new GetAllGroupsForCourseQuery { CourseId = courseId, Language = language };
            var (success, groups, errorMessage) = await _courseService.GetAllGroupsForCourseAsync(query);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(groups);
        }

        [HttpPut("groups/update")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> UpdateGroup([FromBody] UpdateGroupCommand command)
        {
            var (success, errorMessage) = await _courseService.UpdateGroupAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Group updated" });
        }

        [HttpDelete("groups/delete")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> DeleteGroup([FromBody] DeleteGroupCommand command)
        {
            var (success, errorMessage) = await _courseService.DeleteGroupAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Group deleted" });
        }
    }
}