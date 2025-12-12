using Fixtroller.BLL.Services.NumbersServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.Technician
{
    [Route("Api/[area]/[controller]")]
    [ApiController]
    [Area("Technician")]
    [Authorize(Roles = "Technician")]
    public class NumbersController : ControllerBase
    {
        private readonly IMetricsService _metricsService;

        public NumbersController(IMetricsService metricsService)
        {
            _metricsService = metricsService;
        }



        [HttpGet("Technicians/Me/Numbers")]
        public async Task<IActionResult> GetMyNumbers(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("Id")?.Value ?? "";
            var dto = await _metricsService.GetTechnicianDashboardAsync(userId, ct);
            return Ok(dto);
        }

    }
}
