using Fixtroller.BLL.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

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

        // GET: api/MaintenanceManager/Users/Employees
        [HttpGet("Employees")]
        public async Task<IActionResult> GetEmployeesAsync(CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var users = await _userService.GetAllAsync(language, ct);


            var employees = users
                .Where(u => u.RoleName == "Employee" || u.RoleName == "Technician"|| u.RoleName == "Admin")
                .ToList();

            return Ok(new
            {
                message = _localizer["Success"].Value,
                data = employees
            });
        }
    }
}
