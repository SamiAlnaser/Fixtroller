using Fixtroller.BLL.Services.AnnouncementServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.Employee
{
    [Route("Api/[area]/[controller]")]
    [ApiController]
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _service;

        public AnnouncementsController(IAnnouncementService service)
        {
            _service = service;
        }

        private (string userId, string role, string language)? GetContext()
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var role = User.FindFirst("role")?.Value
                    ?? User.FindFirst(ClaimTypes.Role)?.Value
                    ?? string.Empty;

            return (userId, role, language);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            var ctx = GetContext();
            if (ctx is null) return Unauthorized();
            var (userId, role, language) = ctx.Value;

            var result = await _service.GetForUserAsync(
                userId, role, language, search, pageNumber, pageSize, ct);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var ctx = GetContext();
            if (ctx is null) return Unauthorized();
            var (userId, role, language) = ctx.Value;

            var res = await _service.GetByIdForUserAsync(id, userId, role, language, ct);
            if (res is null) return NotFound();

            return Ok(res);
        }
    }
}
