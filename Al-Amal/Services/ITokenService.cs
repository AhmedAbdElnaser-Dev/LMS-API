using System.Security.Claims;
using Al_Amal.Models;

namespace Al_Amal.Services;

public interface ITokenService
{
    string GenerateJwtToken(User user, IList<string> permissions);
    ClaimsPrincipal? ValidateToken(string token);
}