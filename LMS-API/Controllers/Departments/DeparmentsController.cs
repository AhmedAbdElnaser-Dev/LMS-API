using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LMS_API.Controllers.Departments.Commands;

namespace LMS_API.Controllers.Departments
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly DepartmentService _departmentService;

        public DepartmentsController(DepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet("all")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager,Supervisor,Teacher,Student")]
        public async Task<IActionResult> GetDepartments()
        {
            var result = await _departmentService.GetAllDepartments();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager,Supervisor,Teacher,Student")]
        public async Task<IActionResult> GetDepartment(Guid id)
        {
            var result = await _departmentService.GetDepartmentById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentCommand command)
        {
            var result = await _departmentService.CreateDepartment(command);
            return CreatedAtAction(nameof(GetDepartment), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<IActionResult> EditDepartment(Guid id, [FromBody] EditDepartmentCommand command)
        {
            var success = await _departmentService.EditDepartment(id, command);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            var success = await _departmentService.DeleteDepartment(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("translations")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager,Supervisor,Teacher,Student")]
        public async Task<IActionResult> GetTranslations()
        {
            var result = await _departmentService.GetAllTranslations();
            return Ok(result);
        }

        [HttpGet("translations/{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager,Supervisor,Teacher,Student")]
        public async Task<IActionResult> GetTranslation(Guid id)
        {
            var result = await _departmentService.GetTranslationById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("translations")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager,Teacher")]
        public async Task<IActionResult> CreateTranslation([FromBody] CreateDepartmentTranslationCommand command)
        {
            var requiredPermission = $"Translate_{command.Language}";
            if (!User.IsInRole("SuperAdmin") && !User.HasClaim(c => c.Type == "Permission" && c.Value == requiredPermission))
                return Forbid();

            var result = await _departmentService.CreateTranslation(command);
            return CreatedAtAction(nameof(GetTranslation), new { id = result.Id }, result);
        }

        [HttpPut("translations/{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager,Teacher")]
        public async Task<IActionResult> EditTranslation(Guid id, [FromBody] EditDepartmentTranslationCommand command)
        {
            var requiredPermission = $"Translate_{command.Language}";
            if (!User.IsInRole("SuperAdmin") && !User.HasClaim(c => c.Type == "Permission" && c.Value == requiredPermission))
                return Forbid();

            try
            {
                var updatedTranslation = await _departmentService.EditTranslation(id, command);
                return Ok(updatedTranslation);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("translations/{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<IActionResult> DeleteTranslation(Guid id)
        {
            var translation = await _departmentService.GetTranslationById(id);
            if (translation == null)
                return NotFound();

            var requiredPermission = $"Translate_{translation.Language}";
            if (!User.IsInRole("SuperAdmin") && !User.HasClaim(c => c.Type == "Permission" && c.Value == requiredPermission))
                return Forbid();

            var success = await _departmentService.DeleteTranslation(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("users-and-categories")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<IActionResult> GetNonStudentUsersAndCategories()
        {
            var result = await _departmentService.GetNonStudentUsersAndCategories();
            return Ok(result);
        }
    }
}