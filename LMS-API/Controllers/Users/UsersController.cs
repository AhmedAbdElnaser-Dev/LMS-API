using LMS_API.Controllers.Users.Commands;
using LMS_API.Controllers.Users.ViewModels;
using LMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS_API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
                return BadRequest(new { Message = "Invalid input data" });

            var (success, user, errors) = await _userService.RegisterUser(command);

            if (!success)
                return BadRequest(new { Message = "Registration failed", Errors = errors });

            return Ok(new { Message = "User registered successfully", User = user });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { Message = "Invalid email or password" });

            var (success, message) = await _userService.LoginUserAsync(model);

            if (!success)
                return Unauthorized(new { Message = message });

            return Ok(new { Message = message });
        }

        [HttpGet("verify")]
        [Authorize]
        public async Task<IActionResult> VerifyUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "Invalid or missing token" });

            var user = await _userService.VerifyUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            return Ok(user);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{userId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { Message = "User ID is required" });

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            return Ok(user);
        }

        [HttpPost("add")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<IActionResult> AddUser([FromBody] AddUserCommand command)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password) || string.IsNullOrWhiteSpace(command.Role))
                return BadRequest(new { Message = "Invalid input data" });

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Invalid or missing token" });

                var userVm = await _userService.AddUserAsync(userId, command);
                return Ok(new { Message = "User added successfully", User = userVm });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{userId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserCommand command)
        {
            if (string.IsNullOrWhiteSpace(userId) || !ModelState.IsValid)
                return BadRequest(new { Message = "Invalid user ID or input data" });

            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized(new { Message = "Invalid or missing token" });

                var updatedUser = await _userService.UpdateUserAsync(currentUserId, userId, command);
                return Ok(new { Message = "User updated successfully", User = updatedUser });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{userId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { Message = "User ID is required" });

            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized(new { Message = "Invalid or missing token" });

                await _userService.DeleteUserAsync(currentUserId, userId);
                return Ok(new { Message = "User deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("test")]
        [Authorize(Roles = "Student,Teacher")]
        public async Task<IActionResult> Test()
        {
            return Ok("Test");
        }

        [HttpGet("students")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<IActionResult> GetStudents()
        {
            try
            {
                var students = await _userService.GetStudentsAsync();
                return Ok(new { Message = "Students retrieved successfully", Students = students });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Failed to retrieve students: {ex.Message}" });
            }
        }

        [HttpGet("teachers")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<IActionResult> GetTeachers()
        {
            try
            {
                var teachers = await _userService.GetTeachersAsync();
                return Ok(new { Message = "Teachers retrieved successfully", Teachers = teachers });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Failed to retrieve teachers: {ex.Message}" });
            }
        }
    }
}
