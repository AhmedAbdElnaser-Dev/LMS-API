using LMS_API.Controllers.Users.Commands;
using LMS_API.Controllers.Users.ViewModels;
using LMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, user, errors) = await _userService.RegisterUser(command);

            if (!success)
                return BadRequest(errors);

            return Ok(user);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message) = await _userService.LoginUserAsync(model);

            if (!success)
            {
                return Unauthorized(new { Message = message });
            }

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

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddUser([FromBody] AddUserCommand command)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Invalid or missing token" });

                var userVm = await _userService.AddUserAsync(userId, command);

                return Ok(new { Message = "User added successfully", User = userVm });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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
    }
}
