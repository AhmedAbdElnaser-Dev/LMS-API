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
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Courses"))
                return Forbid();

            var (success, courses, errorMessage) = await _courseService.GetAllCoursesDetailedAsync();
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(courses);
        }

        [HttpGet("{courseId}/full-details")]
        public async Task<IActionResult> GetCourseFullDetails(Guid courseId)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Course"))
                return Forbid();

            var (success, course, errorMessage) = await _courseService.GetCourseFullDetailsAsync(courseId);
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(course);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddCourse([FromBody] AddCourseCommand command)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Add_Course"))
                return Forbid();

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
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Remove_Course_Book"))
                return Forbid();

            var result = await _courseService.RemoveBookFromCourseAsync(command);
            if (!result)
                return NotFound("Course or book not found in the course");

            return Ok("Book removed from course successfully");
        }

        [HttpPost("translations/add")]
        public async Task<IActionResult> AddCourseTranslation([FromBody] AddCourseTranslationCommand command)
        {
            var requiredPermissions = new[] { $"Translate_{command.Language}", $"Add_Course_Translate_{command.Language}" };
            if (!await PermissionHelpers.HasAnyPermissionAsync(_context, _httpContextAccessor.HttpContext, requiredPermissions))
                return Forbid();

            var (success, translation, errorMessage) = await _courseService.AddCourseTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Translation added", TranslationId = translation!.Id });
        }

        [HttpPut("translations/edit")]
        public async Task<IActionResult> EditCourseTranslation([FromBody] EditCourseTranslationCommand command)
        {
            var requiredPermissions = new[] { $"Translate_{command.Language}", $"Update_Course_Translate_{command.Language}" };
            if (!await PermissionHelpers.HasAnyPermissionAsync(_context, _httpContextAccessor.HttpContext, requiredPermissions))
                return Forbid();

            var (success, translation, errorMessage) = await _courseService.EditCourseTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Translation updated", TranslationId = translation!.Id });
        }

        [HttpDelete("translations/delete")]
        public async Task<IActionResult> DeleteCourseTranslation([FromBody] DeleteCourseTranslationCommand command)
        {
            var translation = await _courseService.GetCourseTranslationById(command.TranslationId);
            if (translation == null)
                return NotFound(new { Message = "Translation not found" });

            var requiredPermissions = new[] { $"Translate_{translation.Language}", $"Delete_Course_Translate_{translation.Language}" };
            if (!await PermissionHelpers.HasAnyPermissionAsync(_context, _httpContextAccessor.HttpContext, requiredPermissions))
                return Forbid();

            var success = await _courseService.DeleteCourseTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = "Translation not found or could not be deleted" });
            return Ok(new { Message = "Translation deleted" });
        }

        [HttpGet("{courseId}/translation/{language}")]
        public async Task<IActionResult> GetCourseWithTranslation(Guid courseId, string language)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Course"))
                return Forbid();

            var command = new GetCourseWithTranslationQuery { CourseId = courseId, Language = language };
            var (success, course, errorMessage) = await _courseService.GetCourseWithTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(course);
        }

        [HttpGet("all/translation/{language}")]
        public async Task<IActionResult> GetAllCoursesWithTranslation(string language)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Courses"))
                return Forbid();

            var command = new GetAllCoursesWithTranslationQuery { Language = language };
            var (success, courses, errorMessage) = await _courseService.GetAllCoursesWithTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(courses);
        }

        [HttpPost("units/create")]
        public async Task<IActionResult> CreateUnit([FromBody] CreateUnitCommand command)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Add_Unit"))
                return Forbid();

            var (success, unit, errorMessage) = await _courseService.CreateUnitAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Unit created", UnitId = unit!.Id });
        }

        [HttpGet("units/{unitId}/translation/{language}")]
        public async Task<IActionResult> GetUnitWithTranslation(Guid unitId, string language)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Unit"))
                return Forbid();

            var command = new GetUnitWithTranslationQuery { UnitId = unitId, Language = language };
            var (success, unit, errorMessage) = await _courseService.GetUnitWithTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(unit);
        }

        [HttpGet("courses/{courseId}/units/translation/{language}")]
        public async Task<IActionResult> GetAllUnitsForCourse(Guid courseId, string language)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Units"))
                return Forbid();

            var command = new GetAllUnitsForCourseQuery { CourseId = courseId, Language = language };
            var (success, units, errorMessage) = await _courseService.GetAllUnitsForCourseAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(units);
        }

        [HttpPut("units/update")]
        public async Task<IActionResult> UpdateUnit([FromBody] UpdateUnitCommand command)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Update_Unit"))
                return Forbid();

            var (success, errorMessage) = await _courseService.UpdateUnitAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Unit updated" });
        }

        [HttpDelete("units/delete")]
        public async Task<IActionResult> DeleteUnit([FromBody] DeleteUnitCommand command)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Delete_Unit"))
                return Forbid();

            var (success, errorMessage) = await _courseService.DeleteUnitAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Unit deleted" });
        }

        [HttpPost("groups/create")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupCommand command)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Add_Group"))
                return Forbid();

            var (success, group, errorMessage) = await _courseService.CreateGroupAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Group created", GroupId = group!.Id });
        }

        [HttpGet("courses/{courseId}/groups")]
        public async Task<IActionResult> GetCourseGroups(Guid courseId)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Groups"))
                return Forbid();

            var (success, groups, errorMessage) = await _courseService.GetCourseGroupsAsync(courseId);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(groups);
        }

        [HttpGet("groups/{groupId}/details")]
        public async Task<IActionResult> GetGroupDetails(Guid groupId)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Group"))
                return Forbid();

            var (success, group, errorMessage) = await _courseService.GetGroupDetailsWithTranslationsAsync(groupId);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(group);
        }

        [HttpPost("groups/{groupId}/translations")]
        public async Task<IActionResult> AddGroupTranslation(Guid groupId, [FromBody] CreateGroupTranslationCommand command)
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Add_Group_Translation"))
            //    return Forbid();

            command.GroupId = groupId; 
            var (success, translationId, errorMessage) = await _courseService.AddGroupTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Translation added", TranslationId = translationId });
        }

        [HttpGet("groups/{groupId}/translation/{language}")]
        public async Task<IActionResult> GetGroupWithTranslation(Guid groupId, string language)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Group"))
                return Forbid();

            var query = new GetGroupWithTranslationQuery { GroupId = groupId, Language = language };
            var (success, group, errorMessage) = await _courseService.GetGroupWithTranslationAsync(query);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(group);
        }

        [HttpGet("courses/{courseId}/groups/translation/{language}")]
        public async Task<IActionResult> GetAllGroupsForCourse(Guid courseId, string language)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Groups"))
                return Forbid();

            var query = new GetAllGroupsForCourseQuery { CourseId = courseId, Language = language };
            var (success, groups, errorMessage) = await _courseService.GetAllGroupsForCourseAsync(query);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(groups);
        }

        [HttpPut("groups/update")]
        public async Task<IActionResult> UpdateGroup([FromBody] UpdateGroupCommand command)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Update_Group"))
                return Forbid();

            var (success, errorMessage) = await _courseService.UpdateGroupAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Group updated" });
        }

        [HttpPut("groups/{groupId}/translations/{language}")]
        public async Task<IActionResult> UpdateGroupTranslation(Guid groupId, string language, [FromBody] UpdateGroupTranslationCommand command)
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Update_Group_Translation"))
            //    return Forbid();

            if (command.GroupId != groupId || command.Language != language)
                return BadRequest(new { Message = "GroupId or Language in body must match URL parameters." });

            var (success, errorMessage) = await _courseService.UpdateGroupTranslationAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(new { Message = "Group translation updated successfully" });
        }

        [HttpDelete("groups/delete")]
        public async Task<IActionResult> DeleteGroup([FromBody] DeleteGroupCommand command)
        {
            if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Delete_Group"))
                return Forbid();

            var (success, errorMessage) = await _courseService.DeleteGroupAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });
            return Ok(new { Message = "Group deleted" });
        }

        [HttpPost("groups/{groupId}/students")]
        public async Task<IActionResult> AddStudentToGroup(Guid groupId, [FromBody] AddStudentToGroupCommand command)
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Add_Student_To_Group"))
            //    return StatusCode(403, new { Message = "Permission denied" });

            if (command.GroupId != groupId)
                return BadRequest(new { Message = "GroupId in body must match URL parameter." });

            var (success, errorMessage) = await _courseService.AddStudentToGroupAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(new
            {
                Message = "Student added to group successfully",
                GroupId = command.GroupId,
                StudentId = command.StudentId
            });
        }

        [HttpDelete("groups/{groupId}/students/{studentId}")]
        public async Task<IActionResult> RemoveStudentFromGroup(Guid groupId, string studentId, [FromBody] RemoveStudentFromGroupCommand command)
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "Remove_Student_From_Group"))
            //    return StatusCode(403, new { Message = "Permission denied" });

            if (command.GroupId != groupId || command.StudentId != studentId)
                return BadRequest(new { Message = "GroupId or StudentId in body must match URL parameters." });

            var (success, errorMessage) = await _courseService.RemoveStudentFromGroupAsync(command);
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(new
            {
                Message = "Student removed from group successfully",
                GroupId = command.GroupId,
                StudentId = command.StudentId
            });
        }

        [HttpGet("teachers")]
        public async Task<IActionResult> GetTeachers()
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Teachers"))
            //    return Forbid();

            var (success, teachers, errorMessage) = await _courseService.GetTeachersAsync();
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(teachers);
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents()
        {
            //if (!await PermissionHelpers.HasPermissionAsync(_context, _httpContextAccessor.HttpContext, "View_Students"))
            //    return StatusCode(403, new { Message = "Permission denied" });

            var (success, students, errorMessage) = await _courseService.GetStudentsAsync();
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(students);
        }
    }
}