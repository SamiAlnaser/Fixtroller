using Azure.Core;
using Fixtroller.BLL.Services.MaintenanceRequestServices;
using Fixtroller.BLL.Services.TechnicianServices;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> List(
               [FromQuery] int? categoryId,
               [FromQuery] string? search,
               int pageNumber = 1,
               int pageSize = 10,
               CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var data = await _TechnicianService.GetWithMetricsAsync(language, categoryId, search, pageNumber, pageSize, ct);
            return Ok(data);
        }
        [HttpGet("{techId}/assigned")]
        public async Task<IActionResult> GetAssignedForTechnician(
    string techId,
    DateTime? createdFrom = null,
    DateTime? createdTo = null,
    int pageNumber = 1,
    int pageSize = 10,
    int? requestId = null,
    CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var board = await _TechnicianService.GetMyAssignedAsync(
                techId,
                language,
                pageNumber,
                pageSize,
                createdFrom,
                createdTo,
                requestId,
                ct);

            return Ok(board);
        }



        [HttpPost("{id:int}/assign")]
        public async Task<IActionResult> Assign(int id, [FromBody] AssignTechnicianRequestDTO dto, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var (newId, key) = await _requestService.AssignTechnicianAsync(id, dto.TechnicianUserId, dto.ExpectedDuration, language, ct);

            if (newId is null)
                return BadRequest(new { message = _localizer[key].Value });


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
             ?? User.FindFirst("Id")?.Value
             ?? string.Empty;

            var role = User.FindFirst("role")?.Value
                     ?? "Admin"; // قيمة افتراضية آمنة

            var details = await _requestService.GetByIdAsync(id, userId, role, language, ct);

            return CreatedAtRoute("MaintenanceRequest_GetById",
                new { id = newId.Value },
                new
                {
                    message = _localizer[key].Value,
                    data = details
                });
        }


        [HttpPost("{id:int}/assign-list")]
        public async Task<IActionResult> AssignList(int id, [FromBody] AssignTechniciansRequestDTO dto, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var (newId, key) = await _requestService
                .AssignTechniciansAsync(id, dto.TechnicianUserIds, dto.ExpectedDuration, language, ct);

            if (newId is null) return BadRequest(new { message = _localizer[key].Value });


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst("Id")?.Value
                 ?? string.Empty;

            var role = User.FindFirst("role")?.Value
                     ?? "Admin"; // قيمة افتراضية آمنة

            var details = await _requestService.GetByIdAsync(id, userId, role, language, ct);

            return CreatedAtRoute("MaintenanceRequest_GetById",
                new { id = newId.Value },
                new
                {
                    message = _localizer[key].Value,
                    data = details
                });
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
        public async Task<IActionResult> GetByCategory(
            int categoryId,
           [FromQuery] string? search,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var result = await _TechnicianService.GetByCategoryAsync(categoryId, search, language, pageNumber, pageSize, ct);
            return Ok(result);
        }
    }
}
