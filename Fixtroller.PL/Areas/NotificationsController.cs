using Fixtroller.BLL.Services.NotificationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Text.Json;

namespace Fixtroller.PL.Areas
{
    [Route("Api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notifications;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public NotificationsController(INotificationService notifications, IStringLocalizer<SharedResource> localizer)
        {
            _notifications = notifications;
            _localizer = localizer;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirst("Id")?.Value;
        }

        // GET: /api/Notifications?onlyUnread=true
        [HttpGet("")]
        public async Task<IActionResult> Get([FromQuery] bool onlyUnread = false, CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var list = await _notifications.GetForUserAsync(userId, onlyUnread, language, ct);
            return Ok(list);
        }

        // POST: /api/Notifications/{id}/read
        [HttpPost("{id:int}/Read")]
        public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _notifications.MarkAsReadAsync(id, userId, ct);
            return NoContent();
        }

        // POST: /api/Notifications/read-all
        [HttpPost("Read-All")]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _notifications.MarkAllAsReadAsync(userId, ct);
            return NoContent();
        }
    }
}
