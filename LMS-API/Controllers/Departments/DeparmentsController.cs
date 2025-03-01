using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using LMS_API.Controllers.Departments.Commands;

namespace LMS_API.Controllers.Departments
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly DepartmentService _departmentService;

        public DepartmentsController(DepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // Get all Departments
        [HttpGet("all")]
        public async Task<IActionResult> GetDepartments()
        {
            var result = await _departmentService.GetAllDepartments();
            return Ok(result);
        }

        // Get Department by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var result = await _departmentService.GetDepartmentById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Create Department
        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentCommand command)
        {
            var result = await _departmentService.CreateDepartment(command);
            return CreatedAtAction(nameof(GetDepartment), new { id = result.Id }, result);
        }

        // Edit Department
        [HttpPut("{id}")]
        public async Task<IActionResult> EditDepartment(int id, [FromBody] EditDepartmentCommand command)
        {
            var success = await _departmentService.EditDepartment(id, command);
            if (!success) return NotFound();
            return NoContent();
        }

        // Delete Department
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var success = await _departmentService.DeleteDepartment(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // Get All Translations
        [HttpGet("translations")]
        public async Task<IActionResult> GetTranslations()
        {
            var result = await _departmentService.GetAllTranslations();
            return Ok(result);
        }

        // Get Translation by ID
        [HttpGet("translations/{id}")]
        public async Task<IActionResult> GetTranslation(int id)
        {
            var result = await _departmentService.GetTranslationById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Create Translation
        [HttpPost("translations")]
        public async Task<IActionResult> CreateTranslation([FromBody] CreateDepartmentTranslationCommand command)
        {
            var result = await _departmentService.CreateTranslation(command);
            return CreatedAtAction(nameof(GetTranslation), new { id = result.Id }, result);
        }

        // Edit Translation
        [HttpPut("translations/{id}")]
        public async Task<IActionResult> EditTranslation(int id, [FromBody] EditDepartmentTranslationCommand command)
        {
            var success = await _departmentService.EditTranslation(id, command);
            if (!success) return NotFound();
            return NoContent();
        }

        // Delete Translation
        [HttpDelete("translations/{id}")]
        public async Task<IActionResult> DeleteTranslation(int id)
        {
            var success = await _departmentService.DeleteTranslation(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
