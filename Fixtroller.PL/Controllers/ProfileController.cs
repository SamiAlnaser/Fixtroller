using Fixtroller.BLL.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Fixtroller.PL.Controllers
{
    [Route("Api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {   
            private readonly IUserservice _userService;
            private readonly IStringLocalizer<SharedResource> _localizer;

            public ProfileController(IUserservice userService, IStringLocalizer<SharedResource> localizer)
            {
                _userService = userService;
                _localizer = localizer;
            }

            // POST: api/Profile/Image
            [HttpPost("Image")]
            public async Task<IActionResult> UploadImage([FromForm] IFormFile file, CancellationToken ct)
            {
                var userId = User.FindFirst("Id")?.Value
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized();

                var (success, messageKey, imageUrl) =
                    await _userService.UploadMyProfileImageAsync(userId, file, ct);

                var message = _localizer[messageKey].Value;

                if (!success)
                    return BadRequest(new { message });

                return Ok(new
                {
                    message,
                    data = new { imageUrl }
                });
            }

            // GET: api/Profile/Image
            [HttpGet("Image")]
            public async Task<IActionResult> GetImage(CancellationToken ct)
            {
                var userId = User.FindFirst("Id")?.Value
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized();

                var (success, messageKey, imageUrl) =
                    await _userService.GetMyProfileImageAsync(userId, ct);

                var message = _localizer[messageKey].Value;

                if (!success)
                    return BadRequest(new { message });

                return Ok(new
                {
                    message,
                    data = new { imageUrl }
                });
            }

            // DELETE: api/Profile/Image
            [HttpDelete("Image")]
            public async Task<IActionResult> DeleteImage(CancellationToken ct)
            {
                var userId = User.FindFirst("Id")?.Value
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized();

                var (success, messageKey) =
                    await _userService.DeleteMyProfileImageAsync(userId, ct);

                var message = _localizer[messageKey].Value;

                if (!success)
                    return BadRequest(new { message });

                return Ok(new { message });
            }
        }
    }

