using Fixtroller.BLL.Services.MaintenanceRequestServices;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Data;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.MaintenanceManager
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("MaintenanceManager")]
    [Authorize(Roles = "MaintenanceManager")]
    public class MaintenanceRequestController : ControllerBase
    {

        private readonly IMaintenanceRequestService _maintenanceRequestService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public MaintenanceRequestController(IMaintenanceRequestService maintenanceRequestService, IStringLocalizer<SharedResource> localizer)
        {
            _maintenanceRequestService = maintenanceRequestService;
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
                     ?? "MaintenanceManager"; // قيمة افتراضية آمنة

            try
            {
                var res = await _maintenanceRequestService.GetByIdAsync(id, userId, role, language);

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

            var list = await _maintenanceRequestService.GetMineAsync(userId , role);
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role = User.FindFirst("role")?.Value
            ?? User.FindFirst(ClaimTypes.Role)?.Value
            ?? string.Empty;

            var list = await _maintenanceRequestService.GetAllAsync(role);
            return Ok(list);
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