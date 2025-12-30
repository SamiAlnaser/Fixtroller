using Fixtroller.BLL.Services.NumbersServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.MaintenanceManager
{
    [Route("Api/[area]/[controller]")]
    [ApiController]
    [Area("MaintenanceManager")]
    [Authorize(Roles = "MaintenanceManager")]
    public class NumbersController : ControllerBase
    {
        private readonly IMetricsService _metricsService;

        public NumbersController(IMetricsService metricsService)
        {
            _metricsService = metricsService;
        }


        [HttpGet("Technicians/{techId}/Numbers")]
        public async Task<IActionResult> GetTechnicianNumbers(string techId, CancellationToken ct)
        {
            var dto = await _metricsService.GetTechnicianNumbersAsync(techId, ct);
            return Ok(dto);
        }

        [HttpGet("Numbers")]
        public async Task<IActionResult> GetNumbers(CancellationToken ct)
        {
            var dto = await _metricsService.GetManagerDashboardAsync(ct);
            return Ok(dto);
        }

        [HttpGet("Requests/Overview")]
        public async Task<IActionResult> RequestsOverview(
        [FromQuery] DateTimeOffset? fromUtc = null,
        [FromQuery] DateTimeOffset? toUtc = null,
        CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";
            var dto = await _metricsService.GetManagerChartsAsync(language, fromUtc, toUtc, ct);
            return Ok(dto);
        }

        [HttpGet("ManagerAsEmployee/Me/Numbers")]
        public async Task<IActionResult> GetMyNumbersAsEmployee(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("Id")?.Value ?? "";
            var dto = await _metricsService.GetEmployeeDashboardAsync(userId, ct);
            return Ok(dto);
        }

    }
}
