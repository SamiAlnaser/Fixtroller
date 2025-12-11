using Fixtroller.BLL.Services.AiServices;
using Fixtroller.DAL.Data.DTOs.AIDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Fixtroller.PL.Areas.MaintenanceManager
{
    [Area("MaintenanceManager")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize(Roles = "MaintenanceManager")]
    public class AiChatController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;

        public AiChatController(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage(
            [FromBody] AiChatRequestDTO dto,
            CancellationToken ct)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Message_Required");

            var userId =
                User.FindFirst("Id")?.Value
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? string.Empty;

            var userRole =
                User.FindFirst(ClaimTypes.Role)?.Value
                ?? User.FindFirst("role")?.Value
                ?? "MaintenanceManager";

            var reply = await _aiChatService.SendMessageAsync(
                userId,
                userRole,
                dto.Message,
                ct);

            var response = new AiEmployeeChatResponseDTO
            {
                Reply = reply
            };

            return Ok(response);
        }
    }
}
