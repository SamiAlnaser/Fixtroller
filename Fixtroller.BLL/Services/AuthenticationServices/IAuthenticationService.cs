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
        Task<UserResponseDTO> RegisterAsync(RegisterRequestDTO RegisterRequest);
        Task<UserResponseDTO> LoginAsync(LoginRequestDTO LoginRequest);
    }
}
