using Fixtroller.DAL.Data.DTOs.Authentication.Responses;
using Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs;
using Fixtroller.DAL.Data.DTOs.UsersDTOS.Requset;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.UserRepository;
using Mapster;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.UserServices
{
    public class UserService : IUserservice
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<List<UserDTO>> GetAllAsync(
            CancellationToken ct = default)
        {
            var users = await _userRepository.GetAllAsync(ct);
            var userDtos = new List<UserDTO>(users.Count);

            foreach (var user in users)
            {
                ct.ThrowIfCancellationRequested();

                var roles = await _userManager.GetRolesAsync(user);

                userDtos.Add(new UserDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    RoleName = roles.FirstOrDefault() ?? string.Empty
                });
            }

            return userDtos;
        }

        public async Task<UserDTO?> GetByIdAsync(
            string userId,
            CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                RoleName = roles.FirstOrDefault() ?? string.Empty
            };
        }

        public async Task<(bool Success, string MessageKey)> ChangeUserRoleAsync(
            ChangeRoleRequsetDTO dto,
            CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId, ct);
            if (user is null)
                return (false, "User_NotFound");

            var roleName = dto.NewRoleName.ToString();

            var success = await _userRepository.ChangeUserRoleAsync(dto.UserId, roleName, ct);

            if (!success)
                return (false, "User_ChangeRole_Failed");

            return (true, "User_ChangeRole_Success");
        }

        public async Task<(bool Success, string MessageKey)> VacationUserAsync(
            string userId,
            int days,
            CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null)
                return (false, "User_NotFound");

            if (days <= 0)
                return (false, "User_Vacation_InvalidDays");

            var success = await _userRepository.VacationUserAsync(userId, days, ct);
            if (!success)
                return (false, "User_Vacation_Failed");

            return (true, "User_Vacation_Success");
        }

        public async Task<(bool Success, string MessageKey)> UnVacationUserAsync(
            string userId,
            CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null)
                return (false, "User_NotFound");

            var success = await _userRepository.UnVacationUserAsync(userId, ct);
            if (!success)
                return (false, "User_UnVacation_Failed");

            return (true, "User_UnVacation_Success");
        }

        public async Task<(bool IsVacation, string MessageKey)> IsVacationAsync(
            string userId,
            CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null)
                return (false, "User_NotFound");

            var isVacation = await _userRepository.IsVacationAsync(userId, ct);

            return isVacation
                ? (true, "User_IsVacation_True")
                : (false, "User_IsVacation_False");
        }
    }

}
