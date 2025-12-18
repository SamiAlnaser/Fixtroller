using Fixtroller.BLL.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using QuestPDF.Helpers;

namespace Fixtroller.PL.Areas.Technician
{
    [Route("Api/[area]/[controller]")]
    [ApiController]
    [Area("Technician")]
    [Authorize(Roles = "Technician")]
    public class UsersController : ControllerBase
    {
        private readonly IUserservice _userService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public UsersController(IUserservice userService, IStringLocalizer<SharedResource> localizer)
        {
            _userService = userService;
            _localizer = localizer;
        }

        // GET: api/Technician/Users/E0mployees
        [HttpGet("Employees")]
        public async Task<IActionResult> List(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var users = await _userService.GetAllAsync(language, search, pageNumber, pageSize, ct);

            var employees = users.Data
                .Where(u => u.RoleName == "Employee" || u.RoleName == "Admin")
                .ToList();

            var page = new
            {
                users.TotalPages,
                users.CurrentPage,
                users.TotalCount,
                users.PageSize,
                Data = employees.Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.RoleName,
                    u.Email
                })
            };

            return Ok(new
            {
                message = _localizer["Success"].Value,
                data = page
            });
        }
    }
}
