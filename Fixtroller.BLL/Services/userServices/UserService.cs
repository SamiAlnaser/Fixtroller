using Fixtroller.BLL.Services.FileService;
using Fixtroller.DAL.Data.DTOs.Authentication.Responses;
using Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs;
using Fixtroller.DAL.Data.DTOs.PagedResultDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.UsersDTOS.Requset;
using Fixtroller.DAL.Data.DTOs.UsersDTOS.Responses;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.UserRepository;
using Fixtroller.DAL.Repositories.UserRepository.TechnicianRepositorirs;
using Mapster;
using Microsoft.AspNetCore.Http;
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



        private readonly IFileService _fileService;
        private readonly ITechnicianRepository _technicianRepository;

        public UserService(IUserRepository userRepository, IFileService fileService, ITechnicianRepository technicianRepository)
        {
            _userRepository = userRepository;
            _fileService = fileService;
            _technicianRepository = technicianRepository;
        }




        public async Task<List<UserDTO>> GetAllAsync(CancellationToken ct = default)
        {
            var users = await _userRepository.GetAllAsync(ct);
            var userDtos = new List<UserDTO>(users.Count);

            foreach (var user in users)
            {
                ct.ThrowIfCancellationRequested();

                // ✅ السيرفس ما بحكي مع UserManager — الريبو هو اللي برجع الرولز
                var userRoles = await _userRepository.GetRolesAsync(user, ct);

                userDtos.Add(new UserDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    RoleName = userRoles.FirstOrDefault() ?? string.Empty
                });
            }

            return userDtos;
        }


        public async Task<UserDTO?> GetByIdAsync(
       string userId,
       CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null)
                return null;

            // ✅ الريبو هو اللي بجيب الرولز
            var userRoles = await _userRepository.GetRolesAsync(user, ct);

            return new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                RoleName = userRoles.FirstOrDefault() ?? string.Empty
            };
        }


        public async Task<(bool Success, string MessageKey)> ChangeUserRoleAsync(
      ChangeRoleRequsetDTO dto,
      CancellationToken ct = default)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.UserId))
                return (false, "User_NotFound"); // أو Key خاص لو عندك

            var user = await _userRepository.GetByIdAsync(dto.UserId, ct);
            if (user is null)
                return (false, "User_NotFound");

            var roleName = dto.NewRoleName.ToString();
            if (string.IsNullOrWhiteSpace(roleName))
                return (false, "User_ChangeRole_Failed"); // أو Key خاص

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


        public async Task<(bool Success, string MessageKey)> CreateUserByAdminAsync(
            AdminCreateUserRequestDTO dto,
            CancellationToken ct)
        {
            var existing = await _userRepository.GetByEmailAsync(dto.Email, ct);
            if (existing != null)
                return (false, "USER_EMAIL_ALREADY_EXISTS");

            var roleExists = await _userRepository.RoleExistsAsync(dto.Role, ct);
            if (!roleExists)
                return (false, "ROLE_NOT_FOUND");

            var user = new ApplicationUser
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                UserName = dto.Email.Trim(),
                Location = dto.Location.Trim(),
                Department = string.IsNullOrWhiteSpace(dto.Department) ? null : dto.Department.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                EmailConfirmed = true
            };

            var created = await _userRepository.CreateUserAsync(user, dto.Password, ct);
            if (!created)
                return (false, "USER_CREATE_FAILED");

            var roleAdded = await _userRepository.AddToRoleAsync(user, dto.Role, ct);
            if (!roleAdded)
                return (false, "USER_ROLE_ASSIGN_FAILED");

            return (true, "USER_CREATED_SUCCESS");
        }

        public async Task<(bool Success, string MessageKey, string? ImageUrl)> UploadMyProfileImageAsync(
        string userId, IFormFile file, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null)
                return (false, "User_NotFound", null);

            if (file is null || file.Length == 0)
                return (false, "Images_Empty", null);

            try
            {
                // 1) احفظ المسار القديم قبل ما تغيّر أي شيء
                var oldPath = user.ProfileImagePath;

                // 2) ارفع الجديدة
                var newPath = await _fileService.UploadUserAvatarAsync(userId, file, ct);

                // 3) خزّن الجديدة بالداتا بيس
                user.ProfileImagePath = newPath;

                var updated = await _userRepository.UpdateAsync(user, ct);
                if (!updated)
                {
                    // إذا فشل تحديث DB: امسح الصورة الجديدة (تنضيف)
                    await _fileService.DeleteAsync(newPath, ct);
                    return (false, "USER_IMAGE_UPLOAD_FAILED", null);
                }

                // 4) بعد نجاح DB: امسح القديمة (لو موجودة)
                if (!string.IsNullOrWhiteSpace(oldPath))
                    await _fileService.DeleteAsync(oldPath, ct);

                return (true, "USER_IMAGE_UPLOADED_SUCCESS", _fileService.GetPublicUrl(newPath));
            }
            catch (InvalidOperationException ex)
            {
                var msg = ex.Message ?? string.Empty;

                if (msg.Contains("Empty", StringComparison.OrdinalIgnoreCase))
                    return (false, "Images_Empty", null);

                if (msg.Contains("Invalid image type", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("Invalid", StringComparison.OrdinalIgnoreCase))
                    return (false, "Images_InvalidFile", null);

                if (msg.Contains("File too large", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("too large", StringComparison.OrdinalIgnoreCase))
                    return (false, "Images_TooLarge", null);

                return (false, "Images_Upload_Failed", null);
            }
        }


        public async Task<(bool Success, string MessageKey)> DeleteMyProfileImageAsync(
        string userId, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null)
                return (false, "User_NotFound");

            if (string.IsNullOrWhiteSpace(user.ProfileImagePath))
                return (false, "USER_IMAGE_NOT_FOUND");

            try
            {
                await _fileService.DeleteAsync(user.ProfileImagePath, ct);
                user.ProfileImagePath = null;

                var updated = await _userRepository.UpdateAsync(user, ct);
                if (!updated)
                    return (false, "USER_IMAGE_DELETE_FAILED");

                return (true, "USER_IMAGE_DELETED_SUCCESS");
            }
            catch
            {
                return (false, "USER_IMAGE_DELETE_FAILED");
            }
        }

        public async Task<(bool Success, string MessageKey, string? ImageUrl)> GetMyProfileImageAsync(
     string userId, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null) return (false, "User_NotFound", null);

            return (true, "Success",
                string.IsNullOrWhiteSpace(user.ProfileImagePath)
                    ? null
                    : _fileService.GetPublicUrl(user.ProfileImagePath));
        }

        public async Task<PagedResultDTO<AdminTechnicianListItemDTO>> GetTechniciansForAdminAsync(
            string language,
            string? search,
            string? status,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            language = string.IsNullOrWhiteSpace(language) ? "ar" : language;

            var paged = await _technicianRepository.GetPagedAsync(search, status, pageNumber, pageSize, ct);
            var nowUtc = DateTimeOffset.UtcNow;

            var list = paged.Data.Select(u =>
            {
                var categoryName = u.TechnicianCategory?.Translations
                    ?.OrderBy(tr => tr.Language == language ? 0 : tr.Language == "ar" ? 1 : 2)
                    .Select(tr => tr.Name)
                    .FirstOrDefault();

                var isVacation = u.LockoutEnd.HasValue && u.LockoutEnd > nowUtc;

                return new AdminTechnicianListItemDTO
                {
                    Id = u.Id,
                    FullName = u.FullName ?? string.Empty,
                    ProfileImageUrl = string.IsNullOrWhiteSpace(u.ProfileImagePath)
                        ? null
                        : _fileService.GetPublicUrl(u.ProfileImagePath),
                    TechnicianCategoryName = categoryName,
                    IsVacation = isVacation
                };
            }).ToList();

            return new PagedResultDTO<AdminTechnicianListItemDTO>
            {
                TotalPages = paged.TotalPages,
                CurrentPage = paged.CurrentPage,
                TotalCount = paged.TotalCount,
                PageSize = paged.PageSize,
                Data = list
            };
        }

        public async Task<AdminTechniciansAvailabilityNumbersDTO> GetTechniciansAvailabilityNumbersAsync(CancellationToken ct = default)
        {
            var (total, available, vacation) = await _technicianRepository.GetAvailabilityCountsAsync(ct);

            return new AdminTechniciansAvailabilityNumbersDTO
            {
                TotalTechnicians = total,
                AvailableTechnicians = available,
                VacationTechnicians = vacation
            };
        }


    }

}
