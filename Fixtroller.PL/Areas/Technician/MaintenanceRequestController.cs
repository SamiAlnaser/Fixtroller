using Fixtroller.BLL.Services.MaintenanceRequestServices;
using Fixtroller.BLL.Services.TechnicianServices;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.Technician
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Technician")]
    [Authorize(Roles = "Technician")]
    public class MaintenanceRequestController : ControllerBase
    {

        private readonly IMaintenanceRequestService _maintenanceRequestService;
        private readonly ITechnicianService _technicianService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public MaintenanceRequestController(IMaintenanceRequestService maintenanceRequestService, ITechnicianService technicianService, IStringLocalizer<SharedResource> localizer)
        {
            _maintenanceRequestService = maintenanceRequestService;
            _technicianService = technicianService;
            _localizer = localizer;
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromForm] MaintenanceRequestRequestDTO dto)
        {
            var userId = User.FindFirst("Id")?.Value
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var id = await _maintenanceRequestService.CreateWithFile(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, [FromQuery] string language = "ar")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirst("Id")?.Value
                         ?? string.Empty;

            var role = User.FindFirst("role")?.Value
                     ?? "Technician"; // قيمة افتراضية آمنة

            try
            {
                var res = await _maintenanceRequestService.GetByIdAsync(id, userId, role, language);

                // إن رجعت null: إمّا الطلب غير موجود أصلًا، أو الفلترة DB-level منعته.
                // لو بدك تفرّق بدقة بين NotFound و Forbid، استخدم النسخة التحت في الـService.
                return res is null
                    ? NotFound(new { message = _localizer["Request_NotFound"].Value })
                    : Ok(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirst("Id")?.Value
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var role = User.FindFirst("role")?.Value
?? User.FindFirst(ClaimTypes.Role)?.Value
?? string.Empty;

            var list = await _maintenanceRequestService.GetMineAsync(userId, role);
            return Ok(list);
        }

        [HttpGet("assigned")]
        public async Task<IActionResult> GetAssigned()
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirst("Id")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var data = await _technicianService.GetMyAssignedAsync(userId, language);
            return Ok(data);
        }

        [HttpPatch("{id:int}/case")]
        public async Task<IActionResult> ChangeCase(int id, [FromBody] ChangeCaseTypeRequestDTO dto)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("Id")?.Value ?? "";
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var (res, key) = await _maintenanceRequestService.ChangeCaseAsync(id, dto.NewCaseType, userId, role, language);

            if (res is null)
                return BadRequest(new { message = _localizer[key].Value });

            return Ok(new { message = _localizer[key].Value, data = res });
        }

        [HttpPatch("{id:int}/caseMine")]
        public async Task<IActionResult> ChangeCaseMine(int id, [FromBody] ChangeCaseTypeRequestDTO dto)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("Id")?.Value ?? "";
            var role = User.FindFirst("role")?.Value ?? ""; // قد يكون Empty, service يتعامل بمنطق المالك

            var (res, key) = await _maintenanceRequestService.ChangeCaseAsync(id, dto.NewCaseType, userId, role, language);

            if (res is null)
                return BadRequest(new { message = _localizer[key].Value });

            return Ok(new { message = _localizer[key].Value, data = res });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMine(int id, [FromForm] MaintenanceRequestUpdateDTO dto)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var role = User.FindFirst("role")?.Value
                      ?? User.FindFirst(ClaimTypes.Role)?.Value
                      ?? string.Empty;

            try
            {
                var (res, key) = await _maintenanceRequestService.UpdateMineAsync(id, userId, role, dto, language);
                if (res is null) return BadRequest(new { message = _localizer[key].Value });
                return Ok(new { message = _localizer[key].Value, data = res });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

    }
}
