using Azure.Core;
using Fixtroller.BLL.Services.MaintenanceRequestServices;
using Fixtroller.BLL.Services.TechnicianServices;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Data;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.MaintenanceManager
{
    [Route("Api/[area]/[controller]")]
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
        public async Task<IActionResult> List(
               [FromQuery] int? categoryId,
               [FromQuery] string? search,
               [FromQuery] bool excludeCurrentCategory = false,
               int pageNumber = 1,
               int pageSize = 10,
               CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var data = await _TechnicianService.GetWithMetricsAsync(
                language,
                categoryId,
                search,
                pageNumber,
                pageSize,
                excludeCurrentCategory,
                ct);

            return Ok(data);
        }


        [HttpGet("{techId}/Assigned")]
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

        [HttpPost("{id:int}/Assign")]
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
                     ?? "MaintenanceManager"; // قيمة افتراضية آمنة

            var details = await _requestService.GetByIdAsync(id, userId, role, language, ct);

            return CreatedAtRoute("MaintenanceManager_MaintenanceRequest_GetById",
                new { id = newId.Value },
                new
                {
                    message = _localizer[key].Value,
                    data = details
                });
        }
            [HttpPost("{id:int}/Assign-List")]
        public async Task<IActionResult> AssignList(int id, [FromBody] AssignTechniciansRequestDTO dto, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var (newId, key) = dto.IndependentTasks
                ? await _requestService.AssignTechniciansIndependentAsync(id, dto.TechnicianUserIds, dto.ExpectedDuration, language, ct)
                : await _requestService.AssignTechniciansAsync(id,dto.TechnicianUserIds,dto.ExpectedDuration,dto.LeadTechnicianUserId,language,ct);

            if (newId is null) return BadRequest(new { message = _localizer[key].Value });


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
             ?? User.FindFirst("Id")?.Value
             ?? string.Empty;

            var role = User.FindFirst("role")?.Value
                     ?? "MaintenanceManager"; // قيمة افتراضية آمنة

            var details = await _requestService.GetByIdAsync(id, userId, role, language, ct);

            return CreatedAtRoute("MaintenanceManager_MaintenanceRequest_GetById",
                new { id = newId.Value },
                new
                {
                    message = _localizer[key].Value,
                    data = details
                });
        }

        [HttpPost("{id:int}/Group-SharedTask")]
        public async Task<IActionResult> GroupSharedTask(
            int id,
            [FromBody] GroupTechniciansSharedTaskRequestDTO dto,
            CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language))
                language = "ar";

            var (requestId, key) = await _requestService.GroupTechniciansAsSharedTaskAsync(
                id,
                dto.TechnicianUserIds,
                dto.LeadTechnicianUserId,
                ct);

            if (requestId is null)
                return BadRequest(new
                {
                    message = _localizer[key].Value
                });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirst("Id")?.Value
                         ?? string.Empty;

            var role = User.FindFirst("role")?.Value
                       ?? "MaintenanceManager";

            var details = await _requestService.GetByIdAsync(id, userId, role, language, ct);

            return Ok(new
            {
                message = _localizer[key].Value,
                data = details
            });
        }



        [HttpDelete("{id:int}/Technicians/{technicianUserId}")]
        public async Task<IActionResult> RemoveTechnician(
            int id,
            string technicianUserId,
            [FromBody] RemoveTechnicianRequestDTO? dto,
            CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language))
                language = "ar";

            var (requestId, key) = await _requestService.RemoveTechnicianAsync(
                id,
                technicianUserId,
                dto?.NewLeadTechnicianUserId,
                language,
                ct);

            if (requestId is null)
                return BadRequest(new
                {
                    message = _localizer[key].Value
                });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirst("Id")?.Value
                         ?? string.Empty;

            var role = User.FindFirst("role")?.Value
                       ?? "MaintenanceManager";

            var details = await _requestService.GetByIdAsync(id, userId, role, language, ct);

            return Ok(new
            {
                message = _localizer[key].Value,
                data = details
            });
        }

        [HttpGet("{id:int}/Request-Technicians")]
        public async Task<IActionResult> GetRequestTechnicians(int id, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirst("Id")?.Value
                         ?? string.Empty;

            var role = User.FindFirst("role")?.Value
                     ?? "MaintenanceManager";

            var dto = await _requestService.GetRequestTechniciansAsync(
                id,
                userId,
                role,
                language,
                ct);

            if (dto is null)
                return NotFound(new
                {
                    message = _localizer["Request_NotFound"].Value
                });

            return Ok(dto);
        }

        // Patch: Api/MaintenanceManager/Technicians/Category
        [HttpPatch("Category")]
        public async Task<IActionResult> UpdateCategory(
            [FromBody] UpdateTechnicianCategoryRequestDTO dto,
            CancellationToken ct)
        {
            var (ok, key) = await _TechnicianService.UpdateTechnicianCategoryAsync(dto, ct);

            if (!ok)
                return BadRequest(new { message = _localizer[key].Value });


            return Ok(new { message = _localizer[key].Value });
        }

        // DELETE: api/MaintenanceManager/Technicians/{techId}/category
        [HttpDelete("{techId}/Category")]
        public async Task<IActionResult> ClearCategory([FromRoute] string techId, CancellationToken ct)
        {
            var ok = await _TechnicianService.ClearTechnicianCategoryAsync(
                new ClearTechnicianCategoryRequestDTO { TechnicianUserId = techId },
                ct);

            return ok ? NoContent() : BadRequest(new { message = _localizer["BadRequest"].Value });
        }

    }
}
