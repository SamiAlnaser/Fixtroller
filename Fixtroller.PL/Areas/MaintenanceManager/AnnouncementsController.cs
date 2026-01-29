using Fixtroller.BLL.Services.AnnouncementServices;
using Fixtroller.DAL.Data.DTOs.AnnouncementDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.MaintenanceManager
{
    [Area("MaintenanceManager")]
    [Route("Api/[area]/[controller]")]
    [ApiController]
    [Authorize(Roles = "MaintenanceManager")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _service;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AnnouncementsController(
            IAnnouncementService service,
            IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _localizer = localizer;
        }

        private (string userId, string role, string language)? GetContext()
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var role = User.FindFirst("role")?.Value
                    ?? User.FindFirst(ClaimTypes.Role)?.Value
                    ?? string.Empty;

            return (userId, role, language);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            var ctx = GetContext();
            if (ctx is null) return Unauthorized();
            var (userId, role, language) = ctx.Value;

            var result = await _service.GetForUserAsync(
                userId, role, language, search, pageNumber, pageSize, ct);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var ctx = GetContext();
            if (ctx is null) return Unauthorized();
            var (userId, role, language) = ctx.Value;

            var res = await _service.GetByIdForUserAsync(id, userId, role, language, ct);
            if (res is null) return NotFound(new { message = "Announcement_NotFound" });

            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromForm] AnnouncementCreateRequestDTO dto,
            CancellationToken ct)
        {
            var ctx = GetContext();
            if (ctx is null) return Unauthorized();
            var (userId, role, language) = ctx.Value;

            var id = await _service.CreateAsync(dto, userId, role, language, ct);

            return Ok(new
            {
                message = _localizer["Success"].Value,
                id
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] AnnouncementUpdateRequestDTO dto,
            CancellationToken ct)
        {
            var ctx = GetContext();
            if (ctx is null) return Unauthorized();
            var (userId, role, language) = ctx.Value;

            await _service.UpdateAsync(id, dto, userId, role, language, ct);

            return Ok(new { message = _localizer["Success"].Value });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var ctx = GetContext();
            if (ctx is null) return Unauthorized();
            var (userId, role, _) = ctx.Value;

            var ok = await _service.DeleteAsync(id, userId, role, ct);
            if (!ok) return NotFound(new { message = "Announcement_NotFound" });

            return Ok(new { message = _localizer["Success"].Value });
        }
    }
}
