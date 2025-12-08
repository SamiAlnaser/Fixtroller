using Azure.Core;
using Fixtroller.BLL.Services.UserServices;
using Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Fixtroller.PL.Areas.Admin
{
    [Route("api/[area]/[controller]")]
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
            var users = await _userService.GetAllAsync(ct);

            var employees = users
                .Where(u => u.RoleName == "Employee")
                .ToList();

            return Ok(new
            {
                message = _localizer["Success"].Value,
                data = employees
            });
        }

        [HttpPatch("changeRole")]
        public async Task<IActionResult> ChangeRole(
            [FromBody] ChangeRoleRequsetDTO dto,
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
            [FromBody] int days,
            CancellationToken ct)
        {
            var (success, messageKey) = await _userService.VacationUserAsync(userId, days, ct);
            var message = _localizer[messageKey].Value;

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        [HttpPatch("unVacation/{userId}")]
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

        [HttpPatch("isVacation/{userId}")]
        public async Task<IActionResult> IsVacationUser(
            [FromRoute] string userId,
            CancellationToken ct)
        {
            var (isVacation, messageKey) = await _userService.IsVacationAsync(userId, ct);
            var message = _localizer[messageKey].Value;

            return Ok(new
            {
                message,
                data = isVacation
            });
        }
    }
}
