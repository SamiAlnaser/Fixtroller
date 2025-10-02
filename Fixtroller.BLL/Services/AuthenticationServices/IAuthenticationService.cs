using Fixtroller.DAL.Data.DTOs.Authentication.Requests;
using Fixtroller.DAL.Data.DTOs.Authentication.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.AuthenticationServices
{
    public interface IAuthenticationService
    {
        Task<(UserResponseDTO Response, string MessageKey)> RegisterAsync(RegisterRequestDTO RegisterRequest);
        Task<(UserResponseDTO Response, string MessageKey)> LoginAsync(LoginRequestDTO LoginRequest);
    }
}
