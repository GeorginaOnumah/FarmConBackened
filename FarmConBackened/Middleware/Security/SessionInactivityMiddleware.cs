namespace FarmConBackened.Middleware.Security
{
    public class SessionInactivityMiddleware
    {
        private readonly RequestDelegate _next;
        private const int InactivityMinutes = 15;

        public SessionInactivityMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var lastActivityKey = $"last_activity_{context.User.FindFirst("sub")?.Value}";
                var lastActivity = context.Session.GetString(lastActivityKey);

                if (!string.IsNullOrEmpty(lastActivity))
                {
                    var lastActivityTime = DateTime.Parse(lastActivity);
                    if ((DateTime.UtcNow - lastActivityTime).TotalMinutes > InactivityMinutes)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("{\"success\":false,\"message\":\"Session expired due to inactivity.\"}");
                        return;
                    }
                }

                context.Session.SetString(lastActivityKey, DateTime.UtcNow.ToString("O"));
            }

            await _next(context);
        }
    }
}
