using Fixtroller.BLL.Services.FileService;
using Fixtroller.DAL.Data.DTOs.Authentication.Responses;
using Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs;
using Fixtroller.DAL.Data.DTOs.UsersDTOS.Requset;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.UserRepository;
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

        public UserService(IUserRepository userRepository, IFileService fileService)
        {
            _userRepository = userRepository;
            _fileService = fileService;
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

        public async Task<(bool Success, string MessageKey, string? ImageUrl, string? ThumbUrl)> UploadMyProfileImageAsync(
    string userId, IFormFile file, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null)
                return (false, "User_NotFound", null, null);

            // ✅ حماية من null file قبل ما نروح على FileService
            if (file is null || file.Length == 0)
                return (false, "Images_Empty", null, null);

            try
            {
                // احذف القديمة
                if (!string.IsNullOrWhiteSpace(user.ProfileImagePath))
                    await _fileService.DeleteAsync(user.ProfileImagePath, ct);

                if (!string.IsNullOrWhiteSpace(user.ProfileImageThumbPath))
                    await _fileService.DeleteAsync(user.ProfileImageThumbPath, ct);

                // ارفع الجديدة (دويرة + thumb)
                var (imgPath, thumbPath) = await _fileService.UploadUserAvatarAsync(userId, file, ct);

                user.ProfileImagePath = imgPath;
                user.ProfileImageThumbPath = thumbPath;

                var updated = await _userRepository.UpdateAsync(user, ct);
                if (!updated)
                    return (false, "USER_IMAGE_UPLOAD_FAILED", null, null);

                return (true, "USER_IMAGE_UPLOADED_SUCCESS",
                    _fileService.GetPublicUrl(imgPath),
                    _fileService.GetPublicUrl(thumbPath));
            }
            catch (InvalidOperationException ex)
            {
                // ✅ ربط Exceptions تبعت FileService بـ MessageKeys مترجمة
                var msg = ex.Message ?? string.Empty;

                if (msg.Contains("Empty", StringComparison.OrdinalIgnoreCase))
                    return (false, "Images_Empty", null, null);

                if (msg.Contains("Invalid image type", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("Invalid", StringComparison.OrdinalIgnoreCase))
                    return (false, "Images_InvalidFile", null, null);

                if (msg.Contains("File too large", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("too large", StringComparison.OrdinalIgnoreCase))
                    return (false, "Images_TooLarge", null, null);

                return (false, "Images_Upload_Failed", null, null);
            }
        }


        public async Task<(bool Success, string MessageKey)> DeleteMyProfileImageAsync(
     string userId, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null)
                return (false, "User_NotFound");

            var hasAny =
                !string.IsNullOrWhiteSpace(user.ProfileImagePath) ||
                !string.IsNullOrWhiteSpace(user.ProfileImageThumbPath);

            if (!hasAny)
                return (false, "USER_IMAGE_NOT_FOUND"); // إذا بدك تعتبرها Success خبرني

            try
            {
                if (!string.IsNullOrWhiteSpace(user.ProfileImagePath))
                    await _fileService.DeleteAsync(user.ProfileImagePath, ct);

                if (!string.IsNullOrWhiteSpace(user.ProfileImageThumbPath))
                    await _fileService.DeleteAsync(user.ProfileImageThumbPath, ct);

                user.ProfileImagePath = null;
                user.ProfileImageThumbPath = null;

                var updated = await _userRepository.UpdateAsync(user, ct);
                if (!updated)
                    return (false, "USER_IMAGE_DELETE_FAILED");

                return (true, "USER_IMAGE_DELETED_SUCCESS");
            }
            catch (InvalidOperationException)
            {
                // لو DeleteAsync عندك ممكن يرمي (حسب تعديلك/مستقبلاً)
                return (false, "USER_IMAGE_DELETE_FAILED");
            }
            catch (Exception)
            {
                return (false, "USER_IMAGE_DELETE_FAILED");
            }
        }

        public async Task<(bool Success, string MessageKey, string? ImageUrl, string? ThumbUrl)> GetMyProfileImageAsync(
            string userId, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null) return (false, "User_NotFound", null, null);

            return (true, "Success",
                string.IsNullOrWhiteSpace(user.ProfileImagePath) ? null : _fileService.GetPublicUrl(user.ProfileImagePath),
                string.IsNullOrWhiteSpace(user.ProfileImageThumbPath) ? null : _fileService.GetPublicUrl(user.ProfileImageThumbPath));
        }

    }

}
