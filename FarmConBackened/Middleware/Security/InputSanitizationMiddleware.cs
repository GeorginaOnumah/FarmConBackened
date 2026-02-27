using System.Text.RegularExpressions;

namespace FarmConBackened.Middleware.Security
{
    public class InputSanitizationMiddleware
    {
        private readonly RequestDelegate _next;
        // Basic XSS/SQL injection pattern detection
        private static readonly Regex DangerousPatterns = new(
            @"(<script|<\/script|javascript:|on\w+=|';\s*(drop|delete|insert|update|select|exec)|--\s*$|\/\*|\*\/)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public InputSanitizationMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // Check query strings
            foreach (var param in context.Request.Query)
            {
                if (DangerousPatterns.IsMatch(param.Value.ToString()))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("{\"success\":false,\"message\":\"Malicious input detected.\"}");
                    return;
                }
            }

            // For body check, enable buffering
            context.Request.EnableBuffering();
            await _next(context);
        }
    }

}
