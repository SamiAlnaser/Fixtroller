using Fixtroller.DAL.Data.DTOs.Authentication.Responses;
using Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.PagedResultDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.UsersDTOs.Requset;
using Fixtroller.DAL.Data.DTOs.UsersDTOs.Responses;
using Fixtroller.DAL.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.UserServices
{
    public interface IUserservice
    {
        Task<List<UserListItemDTO>> GetAllAsync(string language = "ar", CancellationToken ct = default);
        Task<UserListItemDTO?> GetByIdAsync(string userId, string language = "ar", CancellationToken ct = default);

        Task<(bool Success, string MessageKey)> ChangeUserRoleAsync(
            ChangeRoleRequestDTO dto,
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
        Task<(bool Success, string MessageKey, string? ImageUrl)> UploadMyProfileImageAsync(
            string userId, IFormFile file, CancellationToken ct = default);

        Task<(bool Success, string MessageKey)> DeleteMyProfileImageAsync(
            string userId, CancellationToken ct = default);

        Task<(bool Success, string MessageKey, string? ImageUrl)> GetMyProfileImageAsync(
            string userId, CancellationToken ct = default);


        Task<PagedResultDTO<AdminTechnicianListItemDTO>> GetTechniciansForAdminAsync(
  string language,
  string? search,
  string? status,
  int pageNumber = 1,
  int pageSize = 10,
  CancellationToken ct = default);

        Task<AdminTechniciansAvailabilityNumbersDTO> GetTechniciansAvailabilityNumbersAsync(
    CancellationToken ct = default);

    }


    }
