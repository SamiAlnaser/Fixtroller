using Fixtroller.DAL.Data.DTOs.Authentication.Responses;
using Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs;
using Fixtroller.DAL.Data.DTOs.UsersDTOS.Requset;
using Fixtroller.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.UserServices
{
    public interface IUserservice
    {
        Task<List<UserDTO>> GetAllAsync(
            CancellationToken ct = default);

        Task<UserDTO?> GetByIdAsync(
            string userId,
            CancellationToken ct = default);

        Task<(bool Success, string MessageKey)> ChangeUserRoleAsync(
            ChangeRoleRequsetDTO dto,
            CancellationToken ct = default);

        Task<(bool Success, string MessageKey)> VacationUserAsync(
            string userId,
            int days,
            CancellationToken ct = default);

        Task<(bool Success, string MessageKey)> UnVacationUserAsync(
            string userId,
            CancellationToken ct = default);

        Task<(bool IsVacation, string MessageKey)> IsVacationAsync(
            string userId,
            CancellationToken ct = default);

        Task<(bool Success, string MessageKey)> CreateUserByAdminAsync(
       AdminCreateUserRequestDTO dto,
       CancellationToken ct);
    }
    }
