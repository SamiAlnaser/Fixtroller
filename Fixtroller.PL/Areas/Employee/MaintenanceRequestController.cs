using Fixtroller.BLL.Services.MaintenanceRequestServices;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

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

        [HttpGet("")]
        public async Task<IActionResult> GetAll() => Ok(await _maintenanceRequestService.GetAllAsync());

        [HttpPost("")]
        public async Task<IActionResult> Create([FromForm] MaintenanceRequestRequestDTO request)
        {
            var result = await _maintenanceRequestService.CreateWithFile(request);
            return Ok(result);
        }
    }
}
