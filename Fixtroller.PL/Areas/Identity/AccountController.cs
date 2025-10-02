using Fixtroller.BLL.Services.AuthenticationServices;
using Fixtroller.DAL.Data.DTOs.Authentication.Requests;
using Fixtroller.DAL.Data.DTOs.Authentication.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Fixtroller.PL.Araes.Identity
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Identity")]
    public class AccountController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AccountController(IAuthenticationService authenticationService , IStringLocalizer<SharedResource> localizer)
        {
            _authenticationService = authenticationService;
            _localizer = localizer;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserResponseDTO>> Register(RegisterRequestDTO requestDTO)
        {
            var (response, key) = await _authenticationService.RegisterAsync(requestDTO);
            response.Message = _localizer[key];

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserResponseDTO>> Login(LoginRequestDTO requestDTO)
        {
            var (response, key) = await _authenticationService.LoginAsync(requestDTO);
            response.Message = _localizer[key];

            if (!response.IsSuccess)
                return Unauthorized(response);

            return Ok(response);
        }

    }
}
