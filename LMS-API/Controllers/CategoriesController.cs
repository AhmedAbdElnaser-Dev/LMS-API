using LMS_API.Data;
using LMS_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace LMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin,Manager")]
    public class CategoriesController : ControllerBase
    {
        private readonly DBContext _context;

        public CategoriesController(DBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] String name)
        {
            if (name == "" || string.IsNullOrWhiteSpace(name))
                return BadRequest("Category name is required.");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> EditCategory(Guid id, [FromBody] String name)
        {
            if (name == "" || string.IsNullOrWhiteSpace(name))
                return BadRequest("Category name is required.");

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
                return NotFound("Category not found.");

            existingCategory.Name = name;
            await _context.SaveChangesAsync();

            return Ok(existingCategory);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Category not found.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Category not found.");

            return Ok(category);
        }
    }
}
