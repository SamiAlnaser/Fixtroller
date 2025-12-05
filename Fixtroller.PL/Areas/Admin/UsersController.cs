using Azure.Core;
using Fixtroller.BLL.Services.UserServices;
using Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fixtroller.PL.Areas.Admin
{

    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class UsersController : ControllerBase
    {

        private readonly IUserservice _userService;

        public UsersController(IUserservice userService)
        {
            _userService = userService;
        }

        // GET: api/Admin/Users
        [HttpGet("")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // GET: api/Admin/Users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([FromRoute] string id)
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }

        [HttpPut("ChangeRole")]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleRequsetDTO dto)
        {
            var result = await _userService.ChangeUserRoleAsync(dto);

            return Ok(new { message = "role changed successfully" });
        }

        [HttpPatch("Vacation/{userId}")]

        public async Task<IActionResult> VacationUser([FromRoute] string userId, [FromBody] int days)
        {
            var result = await _userService.VacationUserAsync(userId, days);
            return Ok(result);
        }


        [HttpPatch("unVacation/{userId}")]

        public async Task<IActionResult> UnVacationUser([FromRoute] string userId)
        {
            var result = await _userService.UnVacationUserAsync(userId);
            return Ok(result);
        }
        [HttpPatch("isVacation/{userId}")]

        public async Task<IActionResult> IsVacationUser([FromRoute] string userId)
        {
            var result = await _userService.IsVacationAsync(userId);
            return Ok(result);
        }
    }
}
