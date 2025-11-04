using Fixtroller.BLL.Services.MaintenanceRequestServices;
using Fixtroller.BLL.Services.TechnicianServices;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Fixtroller.PL.Areas.MaintenanceManager
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("MaintenanceManager")]
    [Authorize(Roles = "MaintenanceManager")]
    public class TechniciansController : ControllerBase
    {
        private readonly ITechnicianService _TechnicianService;
        private readonly IMaintenanceRequestService _requestService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public TechniciansController(
            ITechnicianService TechnicianService,
            IMaintenanceRequestService RequestService,
            IStringLocalizer<SharedResource> localizer)
        {
            _TechnicianService = TechnicianService;
            _requestService = RequestService;
            _localizer = localizer;
        }

        [HttpGet] 
        public async Task<IActionResult> List([FromQuery] int? categoryId, [FromQuery] string? search, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var data = await _TechnicianService.GetWithMetricsAsync(language, categoryId, search, ct);
            return Ok(data);
        }


        [HttpPost("{id:int}/assign")]
        public async Task<IActionResult> Assign(int id, [FromBody] AssignTechnicianRequestDTO dto, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var (res, key) = await _requestService.AssignTechnicianAsync(id, dto.TechnicianUserId, language, ct);

            if (res is null)
                return BadRequest(new { message = _localizer[key].Value });

            return Ok(new { message = _localizer[key].Value, data = res });
        }
        [HttpPost("{id:int}/assign-list")]
        public async Task<IActionResult> AssignList(int id, [FromBody] AssignTechniciansRequestDTO dto, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var (res, key) = await _requestService
                .AssignTechniciansAsync(id, dto.TechnicianUserIds, language, ct);

            if (res is null) return BadRequest(new { message = _localizer[key].Value });
            return Ok(new { message = _localizer[key].Value, data = res });
        }

        [HttpDelete("{id:int}/technicians/{techId}")]
        public async Task<IActionResult> RemoveTechnician(int id, string techId, CancellationToken ct)
        {
            var (ok, key) = await _requestService
                .RemoveTechnicianAsync(id, techId, ct);

            if (!ok) return BadRequest(new { message = _localizer[key].Value });
            return Ok(new { message = _localizer[key].Value });
        }

        [HttpPatch("category")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateTechnicianCategoryRequestDTO dto, CancellationToken ct)
        {
            var ok = await _TechnicianService.UpdateTechnicianCategoryAsync(dto, ct);
            return ok ? NoContent() : BadRequest(new { message = _localizer["BadRequest"].Value });
        }

        [HttpGet("by-category/{categoryId:int}")]
        public async Task<IActionResult> GetByCategory(int categoryId, [FromQuery] string? search, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var data = await _TechnicianService.GetByCategoryAsync(categoryId, search, language, ct);
            return Ok(data);
        }
    }
}
