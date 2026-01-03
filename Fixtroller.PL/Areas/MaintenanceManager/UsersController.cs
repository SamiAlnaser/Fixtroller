using Fixtroller.BLL.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using QuestPDF.Helpers;

namespace Fixtroller.PL.Areas.MaintenanceManager
{
    [Route("Api/[area]/[controller]")]
    [ApiController]
    [Area("MaintenanceManager")]
    [Authorize(Roles = "MaintenanceManager")]
    public class UsersController : ControllerBase
    {
        private readonly IUserservice _userService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public UsersController(IUserservice userService, IStringLocalizer<SharedResource> localizer)
        {
            _userService = userService;
            _localizer = localizer;
        }

        [HttpGet("Employees")]
        public async Task<IActionResult> List(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var allowedRoles = new[] { "Employee", "Technician", "Admin" };

            var users = await _userService.GetAllAsync(
                language,
                search,
                pageNumber,
                pageSize,
                allowedRoles,   // ← هاي الجديدة
                ct);

            return Ok(new
            {
                message = _localizer["Success"].Value,
                data = users
            });
        }

        [HttpGet("Technicians")]
        public async Task<IActionResult> GetTechnicians(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            if (pageSize > 100) pageSize = 100;

            var result = await _userService.GetTechniciansForAdminAsync(language, search, status, pageNumber, pageSize, ct);

            var items = result.Data.Select(x => new
            {
                x.Id,
                x.FullName,
                x.ProfileImageUrl,
                TechnicianCategoryName = x.TechnicianCategoryName,
                Status = x.IsVacation
                    ? _localizer["Technician_Status_Vacation"].Value
                    : _localizer["Technician_Status_Available"].Value
            }).ToList();

            return Ok(new
            {
                message = _localizer["Success"].Value,
                data = new
                {
                    result.TotalPages,
                    result.CurrentPage,
                    result.TotalCount,
                    result.PageSize,
                    Data = items
                }
            });
        }

        [HttpGet("Technicians/Numbers")]
        public async Task<IActionResult> GetTechniciansNumbers(CancellationToken ct)
        {
            var dto = await _userService.GetTechniciansAvailabilityNumbersAsync(ct);

            return Ok(new
            {
                message = _localizer["Success"].Value,
                data = dto
            });
        }

    }
}
