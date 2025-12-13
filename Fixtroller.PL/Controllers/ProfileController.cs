using Fixtroller.BLL.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Fixtroller.PL.Controllers
{
    [Route("api/[controller]")]
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

                var (success, messageKey, imageUrl, thumbUrl) =
                    await _userService.UploadMyProfileImageAsync(userId, file, ct);

                var message = _localizer[messageKey].Value;

                if (!success)
                    return BadRequest(new { message });

                return Ok(new
                {
                    message,
                    data = new { imageUrl, thumbUrl }
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

                var (success, messageKey, imageUrl, thumbUrl) =
                    await _userService.GetMyProfileImageAsync(userId, ct);

                var message = _localizer[messageKey].Value;

                if (!success)
                    return BadRequest(new { message });

                return Ok(new
                {
                    message,
                    data = new { imageUrl, thumbUrl }
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

