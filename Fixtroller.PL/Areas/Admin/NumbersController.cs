using Fixtroller.BLL.Services.NumbersServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.Admin
{
    [Route("Api/[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NumbersController : ControllerBase
    {
        private readonly IMetricsService _metricsService;

        public NumbersController(IMetricsService metricsService )
        {
            _metricsService = metricsService;
        }


        [HttpGet("Technicians/{techId}/Numbers")]
        public async Task<IActionResult> GetTechnicianNumbers(string techId, CancellationToken ct)
        {
            var dto = await _metricsService.GetTechnicianNumbersAsync(techId, ct);
            return Ok(dto);
        }

        [HttpGet("Admin/me/Numbers")]
        public async Task<IActionResult> GetMyNumbers(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("Id")?.Value ?? "";
            var dto = await _metricsService.GetEmployeeDashboardAsync(userId, ct);
            return Ok(dto);
        }

    }
}
