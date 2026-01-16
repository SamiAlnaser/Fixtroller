using Fixtroller.BLL.Services.NotificationServices;
using Fixtroller.PL.Services.Notifications.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Sockets;

namespace Fixtroller.PL.Areas
{
    [ApiController]
    [Route("api/diagnostics")]
    public class DiagnosticsEmailController : ControllerBase
    {
        private readonly IAppEmailSender _sender;

        public DiagnosticsEmailController(IAppEmailSender sender)
        {
            _sender = sender;
        }

        [HttpPost("send-test")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendTest([FromQuery] string to, CancellationToken ct)
        {
            try
            {
                var ok = await _sender.SendAsync(to, "Fixtroller Test", "<b>Hello from Fixtroller</b>", ct);
                return Ok(new { ok });
            }
            catch (Exception ex)
            {
                return Ok(new { ok = false, error = ex.Message, type = ex.GetType().FullName });
            }
        }
    }
}
