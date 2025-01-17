using Al_Amal.DTOs;
using Al_Amal.Services;
using Microsoft.AspNetCore.Mvc;

namespace Al_Amal.Controllers
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
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RegisterUserAsync(userDto);
            if (!result)
                return Conflict(new { message = "Email already exists." });

            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.AuthenticateUserAsync(loginDto);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(new { message = "Login successful.", user });
        }
    }
}
