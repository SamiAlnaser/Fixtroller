using Fixtroller.BLL.Services.MaintenanceRequestServices;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.Employee
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Employee")]
    [Authorize(Roles = "Employee , SpecialEmployee")]
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
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _maintenanceRequestService.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirst("Id")?.Value
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var list = await _maintenanceRequestService.GetMineAsync(userId);
            return Ok(list);
        }
    }
}
