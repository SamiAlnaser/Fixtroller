using Fixtroller.BLL.Services.NumbersServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Fixtroller.PL.Areas.MaintenanceManager
{
    [Route("api/[area]/[controller]")]
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


        [HttpGet("technicians/{techId}/numbers")]
        public async Task<IActionResult> GetTechnicianNumbers(string techId, CancellationToken ct)
        {
            var dto = await _metricsService.GetTechnicianNumbersAsync(techId, ct);
            return Ok(dto);
        }

        [HttpGet("numbers")]
        public async Task<IActionResult> GetNumbers(CancellationToken ct)
        {
            var dto = await _metricsService.GetManagerDashboardAsync(ct);
            return Ok(dto);
        }

        [HttpGet("requests/overview")]
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
    }
}
