using Fixtroller.BLL.Services.AiServices;
using Fixtroller.DAL.Data.DTOs.AIDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.Admin
{
    [Route("Api/[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AiChatController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;

        private readonly IStringLocalizer<SharedResource> _localizer;

        public AiChatController(IAiChatService aiChatService, IStringLocalizer<SharedResource> localizer)
        {
            _aiChatService = aiChatService;
            _localizer = localizer;
        }

        // POST: api/Admin/AiChat
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

            // الدور هنا ثابت "Admin"
            var result = await _aiChatService.SendAsync(
                "Admin",
                dto.Message,
                dto.History,
                ct);

            return Ok(new
            {
                message = _localizer["Success"].Value,
                data = result
            });
        }

        // GET: api/Admin/AiChatSettings
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

        // PUT: api/Admin/AiChatSettings
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
