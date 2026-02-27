using System.Security.Claims;

namespace FarmConBackened.Helpers.Extensions
{
    public static class ClaimsExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;
            return sub != null ? Guid.Parse(sub) : Guid.Empty;
        }

        public static string GetRole(this ClaimsPrincipal user) =>
            user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        public static bool IsAdmin(this ClaimsPrincipal user) =>
            user.IsInRole("Admin");
    }
}
