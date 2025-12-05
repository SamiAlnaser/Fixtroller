using Fixtroller.BLL.Services.NotificationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fixtroller.PL.Areas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notifications;

        public NotificationsController(INotificationService notifications)
        {
            _notifications = notifications;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirst("Id")?.Value;
        }

        // GET: /api/Notifications?onlyUnread=true
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] bool onlyUnread, CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var data = await _notifications.GetForUserAsync(userId, onlyUnread, ct);
            return Ok(data);
        }

        // POST: /api/Notifications/{id}/read
        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _notifications.MarkAsReadAsync(id, userId, ct);
            return NoContent();
        }

        // POST: /api/Notifications/read-all
        [HttpPost("read-all")]
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
