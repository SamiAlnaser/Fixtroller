using Fixtroller.BLL.Services.AiServices;
using Fixtroller.DAL.Data.DTOs.AIDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Fixtroller.PL.Areas.Technician
{
    [Area("Technician")]
    [Route("Api/[area]/[controller]")]
    [ApiController]
    [Authorize(Roles = "Technician")]
    public class AiChatController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;

        private readonly IStringLocalizer<SharedResource> _localizer;

        public AiChatController(IAiChatService aiChatService, IStringLocalizer<SharedResource> localizer)
        {
            _aiChatService = aiChatService;
            _localizer = localizer;
        }


        // POST: api/Technician/AiChat
        [HttpPost]
        public async Task<IActionResult> SendAsync(
            [FromBody] AiEmployeeChatRequestDTO dto,
            CancellationToken ct)
        {
            var userId =
                User.FindFirst("Id")?.Value ??
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                string.Empty;

            // الدور هنا "Technician"
            var result = await _aiChatService.SendAsync(
                "Technician",
                dto.Message,
                dto.History,
                ct);

            return Ok(new
            {
                message = _localizer["Success"].Value,
                data = result
            });
        }

    }
}
