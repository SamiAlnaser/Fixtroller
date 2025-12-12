using Fixtroller.BLL.Services.AiServices;
using Fixtroller.DAL.Data.DTOs.AIDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Fixtroller.PL.Areas.MaintenanceManager
{
    [Area("MaintenanceManager")]
    [Route("Api/[area]/[controller]")]
    [ApiController]
    [Authorize(Roles = "MaintenanceManager")]
    public class AiChatController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;

        private readonly IStringLocalizer<SharedResource> _localizer;

        public AiChatController(IAiChatService aiChatService, IStringLocalizer<SharedResource> localizer)
        {
            _aiChatService = aiChatService;
            _localizer = localizer;
        }

        // POST: api/MaintenanceManager/AiChat
        [HttpPost]
        public async Task<IActionResult> SendAsync(
            [FromBody] AiEmployeeChatRequestDTO dto,
            CancellationToken ct)
        {
            // نجيب الـ userId من التوكن
            var userId =
                User.FindFirst("Id")?.Value ??
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                string.Empty;

            // الدور هنا ثابت "MaintenanceManager"
            var result = await _aiChatService.SendAsync(
                "MaintenanceManager",
                dto.Message,
                dto.History,
                ct);

            return Ok(new
            {
                message = _localizer["Success"].Value,
                data = result
            });
        }

        // GET: api/MaintenanceManager/AiChatSettings
        [HttpGet]
        public async Task<IActionResult> GetAsync(CancellationToken ct)
        {
            var settings = await _aiChatService.GetSettingsAsync(ct);

            return Ok(new
            {
                message = _localizer["Success"].Value,
                data = settings
            });
        }

        // PUT: api/MaintenanceManager/AiChatSettings
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(
            [FromBody] AiEmployeeChatSettingsDTO dto,
            CancellationToken ct)
        {
            var updated = await _aiChatService.UpdateSettingsAsync(
                dto.IsEmployeeEnabled,
                dto.IsTechnicianEnabled,
                ct);

            return Ok(new
            {
                message = _localizer["Success"].Value,
                data = updated
            });
        }
    }
}
