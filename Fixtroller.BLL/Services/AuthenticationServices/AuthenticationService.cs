using Fixtroller.DAL.Data.DTOs.Authentication.Requests;
using Fixtroller.DAL.Data.DTOs.Authentication.Responses;
using Fixtroller.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.AuthenticationServices
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthenticationService(UserManager<ApplicationUser> userManager, IConfiguration configuration) 
        {
            _userManager = userManager;
            _configuration = configuration;
        }
        public async Task<(UserResponseDTO Response, string MessageKey)> LoginAsync(LoginRequestDTO loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (user == null)
            {
                return (new UserResponseDTO
                {
                    Token = null,
                    IsSuccess = false,
                    Message = null
                }, "InvalidCredentials");
            }


            return (new UserResponseDTO
            {
                Token = await CreateTokenAsync(user),
                IsSuccess = true,
                Message = null
            }, "LoginSuccess");
        }


        public async Task<(UserResponseDTO Response, string MessageKey)> RegisterAsync(RegisterRequestDTO RegisterRequest)
        {
            var existingUser = await _userManager.FindByEmailAsync(RegisterRequest.Email);
            if (existingUser != null)
            {
                return (new UserResponseDTO
                {
                    IsSuccess = false,
                    Message = null
                }, "EmailAlreadyExists");
            }
            var user = new ApplicationUser()
            {
                FullName = RegisterRequest.FullName,
                Email = RegisterRequest.Email,
                PhoneNumber = RegisterRequest.PhoneNumber,
                UserName = RegisterRequest.UserName ?? RegisterRequest.Email,
                Location = RegisterRequest.Location 
            };

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                return (new UserResponseDTO
                {
                    IsSuccess = false,
                    Message = string.Join("; ", result.Errors.Select(e => e.Description))
                }, "RegisterFailed");
            }

            await _userManager.AddToRoleAsync(user, "Employee");
            return (new UserResponseDTO
            {
                Token = await CreateTokenAsync(user),
                IsSuccess = true,
                Message = null
            }, "RegisterSuccess");
        }

        private async Task<string> CreateTokenAsync(ApplicationUser user)
        {
            var Claims = new List<Claim>()
             {
        new Claim("Email", user.Email),
        new Claim("Name", user.UserName),//
        new Claim("Id", user.Id.ToString()),
        new Claim("PhoneNumber", user.PhoneNumber),
        new Claim("Location", user.Location)
             };

            var Roles = await _userManager.GetRolesAsync(user);
            foreach (var role in Roles)
            {
                Claims.Add(new Claim("role", role));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("jwtOptions")["SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: Claims,
                expires: DateTime.Now.AddDays(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
