using Fixtroller.BLL.Services.NotificationServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fixtroller.PL.Areas
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        // GET: api/System/test-email
        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail(
            [FromServices] IAppEmailSender emailSender)
        {
            // حط إيميلك الحقيقي هون عشان تشوف الرسالة
            var to = "samialnser@gmail.com";

            await emailSender.SendAsync(
                to,
                "Test email from Fixtroller",
                "هذا إيميل تجريبي من نظام الصيانة.");

            return Ok("Email sent (لو الإعدادات صح 😄)");
        }
    }
}
