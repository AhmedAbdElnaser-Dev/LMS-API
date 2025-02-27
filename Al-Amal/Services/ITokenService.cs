using System.Security.Claims;
using Al_Amal.Models;

namespace Al_Amal.Services;

public interface ITokenService
{
    string GenerateJwtToken(ApplicationUser user, IList<string> roles, IList<string> permissions);
    ClaimsPrincipal? ValidateToken(string token);
}