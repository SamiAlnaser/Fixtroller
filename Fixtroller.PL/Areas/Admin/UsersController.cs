using Azure.Core;
using Fixtroller.BLL.Services.UserServices;
using Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.UsersDTOs.Requset;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Fixtroller.PL.Areas.Admin
{
    [Route("Api/[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserservice _userService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public UsersController(
            IUserservice userService,
            IStringLocalizer<SharedResource> localizer)
        {
            _userService = userService;
            _localizer = localizer;
        }

        [HttpGet("Employees")]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var users = await _userService.GetAllAsync(ct);

            var employees = users.ToList();

            return Ok(new
            {
                message = _localizer["Success"].Value,
                data = employees
            });
        }

        [HttpPatch("ChangeRole")]
        public async Task<IActionResult> ChangeRole(
            [FromBody] ChangeRoleRequestDTO dto,
            CancellationToken ct)
        {
            var (success, messageKey) = await _userService.ChangeUserRoleAsync(dto, ct);
            var message = _localizer[messageKey].Value;

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }


        [HttpPatch("Vacation/{userId}")]
        public async Task<IActionResult> VacationUser(
            [FromRoute] string userId,
            [FromBody] VacationUserRequestDTO dto,
            CancellationToken ct)
        {
            var (success, messageKey) = await _userService.VacationUserAsync(userId, dto.Days, ct);
            var message = _localizer[messageKey].Value;

            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpPatch("UnVacation/{userId}")]
        public async Task<IActionResult> UnVacationUser(
            [FromRoute] string userId,
            CancellationToken ct)
        {
            var (success, messageKey) = await _userService.UnVacationUserAsync(userId, ct);
            var message = _localizer[messageKey].Value;

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
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

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] AdminCreateUserRequestDTO dto, CancellationToken ct)
        {
            var (success, messageKey) = await _userService.CreateUserByAdminAsync(dto, ct);
            var message = _localizer[messageKey].Value;

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

    }
}
