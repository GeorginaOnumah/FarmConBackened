using FarmConBackened.Helpers.Extensions;
using FarmConBackened.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace FarmConBackened.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public abstract class BaseController : ControllerBase
    {
        protected Guid CurrentUserId => User.GetUserId();
        protected string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        protected string GetUserAgent() => Request.Headers.UserAgent.ToString();
    }
}
