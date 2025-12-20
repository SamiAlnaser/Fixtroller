using Fixtroller.BLL.Services.NotificationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fixtroller.PL.Areas
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        [HttpGet("debug")]
        [Authorize]
        public IActionResult DebugMe()
        {
            return Ok(new
            {
                User.Identity?.IsAuthenticated,
                User.Identity?.Name,
                Roles = User.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                    .Select(c => new { c.Type, c.Value })
                    .ToList()
            });
        }
    }
}
