using LMS_API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMS_API.Helpers
{
    public static class PermissionHelpers
    {
        public static async Task<bool> HasPermissionAsync(DBContext context, HttpContext httpContext, string permission)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return false;

            var roles = httpContext.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!roles.Any())
                return false;

            return await context.RolePermissions
                .AnyAsync(rp => roles.Contains(rp.Role.Name) && rp.Permission.Name == permission);
        }

        public static async Task<bool> HasAnyPermissionAsync(DBContext context, HttpContext httpContext, IEnumerable<string> permissions)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return false;

            var roles = httpContext.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!roles.Any())
                return false;

            return await context.RolePermissions
                .AnyAsync(rp => roles.Contains(rp.Role.Name) && permissions.Contains(rp.Permission.Name));
        }
    }
}