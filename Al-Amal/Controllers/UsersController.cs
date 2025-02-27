using Al_Amal.DTOs;
using Al_Amal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Al_Amal.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDTO userDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (result, user) = await _userService.RegisterUserAsync(userDto);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Registration successful." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (user, token) = await _userService.AuthenticateUserAsync(loginDto);
        if (user == null || token == null)
            return Unauthorized(new { message = "Invalid email or password." });

        Response.Cookies.Append("jwt_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });

        var userResponse = new UserResponseDTO
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Age = user.Age,
            Email = user.Email ?? string.Empty,
            Gender = user.Gender,
            PhoneNumber = user.PhoneNumber,
            TelegramNumber = user.TelegramNumber,
            Timezone = user.Timezone,
            Country = user.Country,
            RoleName = (await _userService.GetUserRolesAsync(user)).FirstOrDefault() ?? string.Empty
        };

        return Ok(new { message = "Login successful.", user = userResponse });
    }

    [Authorize]
    [HttpGet("verify")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var user = await _userService.GetUserByIdAsync(userGuid);
        if (user == null)
            return NotFound();

        var roles = await _userService.GetUserRolesAsync(user);
        var permissions = await _userService.GetUserPermissionsAsync(user);

        var userResponse = new UserResponseDTO
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Age = user.Age,
            Email = user.Email ?? string.Empty,
            Gender = user.Gender,
            PhoneNumber = user.PhoneNumber,
            TelegramNumber = user.TelegramNumber,
            Timezone = user.Timezone,
            Country = user.Country,
            RoleName = roles.FirstOrDefault() ?? string.Empty
        };

        return Ok(new { user = userResponse, permissions });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt_token");
        return Ok(new { message = "Logged out successfully." });
    }
}