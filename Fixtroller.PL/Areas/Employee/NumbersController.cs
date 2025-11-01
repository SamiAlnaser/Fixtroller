using Fixtroller.BLL.Services.NumbersServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.Employee
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Employee")]
    [Authorize(Roles = "Employee , SpecialEmployee")]
    public class NumbersController : ControllerBase
    {
        private readonly IMetricsService _metricsService;

        public NumbersController(IMetricsService metricsService)
        {
            _metricsService = metricsService;
        }

        [HttpGet("employees/me/numbers")]
        public async Task<IActionResult> GetMyNumbers(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("Id")?.Value ?? "";
            var dto = await _metricsService.GetEmployeeDashboardAsync(userId, ct);
            return Ok(dto);
        }

    }
}
