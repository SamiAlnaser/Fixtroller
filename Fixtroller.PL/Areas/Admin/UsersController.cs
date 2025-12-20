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
        public async Task<IActionResult> List(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var result = await _userService.GetAllAsync(language, search, pageNumber, pageSize, ct);

            // نجهّز ليست الصفحة (page object)
            var page = new
            {
                result.TotalPages,
                result.CurrentPage,
                result.TotalCount,
                result.PageSize,
                Data = result.Data.Select(u => new
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

        [HttpPatch("ChangeRole")]
        public async Task<IActionResult> ChangeRole(
            [FromBody] ChangeRoleRequestDTO dto,
            CancellationToken ct)
        {
            var (ok, key) = await _userService.ChangeUserRoleAsync(dto, ct);
            if (!ok)
                return BadRequest(new { message = _localizer[key].Value });

            return Ok(new { message = _localizer[key].Value });
        }

        [HttpPatch("Vacation/{userId}")]
        public async Task<IActionResult> VacationUser(
            [FromRoute] string userId,
            [FromBody] VacationUserRequestDTO dto,
            CancellationToken ct)
        {
            var (ok, key) = await _userService.VacationUserAsync(userId, dto.Days, ct);
            if (!ok)
                return BadRequest(new { message = _localizer[key].Value });

            return Ok(new { message = _localizer[key].Value });
        }

        [HttpPatch("UnVacation/{userId}")]
        public async Task<IActionResult> UnVacationUser(
            [FromRoute] string userId,
            CancellationToken ct)
        {
            var (ok, key) = await _userService.UnVacationUserAsync(userId, ct);
            if (!ok)
                return BadRequest(new { message = _localizer[key].Value });

            return Ok(new { message = _localizer[key].Value });
        }

        //[HttpGet("IsVacation/{userId}")]
        //public async Task<IActionResult> IsVacationUser(
        //    [FromRoute] string userId,
        //    CancellationToken ct)
        //{
        //    var (isVacation, key) = await _userService.IsVacationAsync(userId, ct);

        //    if (string.Equals(key, "User_NotFound", StringComparison.OrdinalIgnoreCase))
        //        return BadRequest(new { message = _localizer[key].Value });

        //    return Ok(new
        //    {
        //        message = _localizer[key].Value,
        //        data = new { isVacation }
        //    });
        //}

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
            var (ok, key) = await _userService.CreateUserByAdminAsync(dto, ct);
            if (!ok)
                return BadRequest(new { message = _localizer[key].Value });

            return Ok(new { message = _localizer[key].Value });
        }

    }
}
