using Fixtroller.BLL.Mapping;
using Fixtroller.BLL.Services.FileService;
using Fixtroller.BLL.Services.NotificationServices;
using Fixtroller.DAL.Data.DTOs.AnnouncementDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.AnnouncementDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.PagedResultDTOs.Responses;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.Announcements;
using Fixtroller.DAL.Repositories.AnnouncementRepositories;
using Fixtroller.DAL.Repositories.UserRepository;
using Fixtroller.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.AnnouncementServices
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _uow;

        public AnnouncementService(
            IAnnouncementRepository repo,
            IUserRepository userRepo,
            IFileService fileService,
            INotificationService notificationService,
            IUnitOfWork uow)
        {
            _repo = repo;
            _userRepo = userRepo;
            _fileService = fileService;
            _notificationService = notificationService;
            _uow = uow;
        }

        private static bool CanManage(string role)
            => role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);

        public async Task<int> CreateAsync(
            AnnouncementCreateRequestDTO dto,
            string creatorUserId,
            string creatorRole,
            string language,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (!CanManage(creatorRole))
                throw new UnauthorizedAccessException("Forbidden");

            var entity = new Announcement
            {
                Title = dto.Title.Trim(),
                Content = dto.Content.Trim(),
                LinkUrl = string.IsNullOrWhiteSpace(dto.LinkUrl) ? null : dto.LinkUrl.Trim(),
                Audience = dto.Audience,
                CreatedByUserId = creatorUserId
            };

            await _repo.AddAsync(entity, ct);

            // حفظ أولي حتى يصير عنده Id
            await _uow.SaveAndCommitAsync(ct);

            // رفع الصور إن وجدت
            if (dto.Images is { Count: > 0 })
            {
                foreach (var file in dto.Images)
                {
                    if (file is null || file.Length == 0) continue;

                    var fileName = await _fileService.UploadAsync(file, ct);

                    entity.Images.Add(new AnnouncementImage
                    {
                        AnnouncementId = entity.Id,
                        FileName = fileName
                    });
                }

                await _uow.SaveAndCommitAsync(ct);
            }

            // لو مخصص للفنيين → إشعار
            if (entity.Audience == AnnouncementAudience.TechniciansOnly)
            {
                var technicians = await _userRepo.GetByRoleAsync("Technician", ct);

                foreach (var tech in technicians)
                {
                    await _notificationService.CreateAsync(
                        new NotificationCreateModel
                        {
                            UserId = tech.Id,
                            MaintenanceRequestId = null,
                            TitleKey = "Notifications:TechAnnouncement_Title",
                            BodyKey = "Notifications:TechAnnouncement_Body",
                            TitleArgs = new object[] { entity.Title },
                            BodyArgs = new object[] { entity.Title },
                            Language = language,
                            Type = NotificationType.General,
                            Severity = NotificationSeverity.Info,
                            Channels = NotificationChannel.InApp
                        },
                        ct);
                }
            }

            return entity.Id;
        }

        public async Task<int> UpdateAsync(
     int id,
     AnnouncementUpdateRequestDTO dto,
     string userId,
     string userRole,
     string language,
     CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (!CanManage(userRole))
                throw new UnauthorizedAccessException("Forbidden");

            var entity = await _repo.Query(
                    asTracking: true,
                    include: q => q.Include(a => a.Images),
                    predicate: a => a.Id == id)
                .FirstOrDefaultAsync(ct);

            if (entity is null)
                throw new KeyNotFoundException("Announcement_NotFound");

            // تحديث البيانات النصية
            entity.Title = dto.Title.Trim();
            entity.Content = dto.Content.Trim();
            entity.LinkUrl = string.IsNullOrWhiteSpace(dto.LinkUrl)
                ? null
                : dto.LinkUrl.Trim();
            entity.Audience = dto.Audience;

            // 1) حذف الصور المطلوبة
            if (dto.DeletedImageIds is { Count: > 0 })
            {
                var toDelete = entity.Images
                    .Where(i => dto.DeletedImageIds.Contains(i.Id))
                    .ToList();

                foreach (var img in toDelete)
                {
                    // لو عندك Delete في خدمة الملفات استعمله
                    // await _fileService.DeleteAsync(img.FileName, ct);

                    entity.Images.Remove(img);
                }
            }

            // 2) إضافة صور جديدة
            if (dto.NewImages is { Count: > 0 })
            {
                foreach (var file in dto.NewImages)
                {
                    if (file is null || file.Length == 0) continue;

                    var fileName = await _fileService.UploadAsync(file, ct);

                    entity.Images.Add(new AnnouncementImage
                    {
                        FileName = fileName,
                        AnnouncementId = entity.Id
                    });
                }
            }

            await _repo.UpdateAsync(entity, ct);
            await _uow.SaveAndCommitAsync(ct);

            return entity.Id;
        }

        public async Task<bool> DeleteAsync(
            int id,
            string userId,
            string userRole,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (!CanManage(userRole))
                throw new UnauthorizedAccessException("Forbidden");

            var entity = await _repo.GetByIdAsync(id, asTracking: true, ct);
            if (entity is null) return false;

            await _repo.RemoveAsync(entity, ct);
            await _uow.SaveAndCommitAsync(ct);

            return true;
        }

        public async Task<PagedResultDTO<AnnouncementListItemDTO>> GetForUserAsync(
            string userId,
            string userRole,
            string language,
            string? search,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            // فلترة حسب الدور
            bool isAdminOrManager = CanManage(userRole);
            bool isTechnician = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            bool isEmployee = userRole.Equals("Employee", StringComparison.OrdinalIgnoreCase);

            var q = _repo.Query(
                asTracking: false,
                include: a => a
                    .Include(x => x.Images)
                    .Include(x => x.CreatedByUser));

            if (!isAdminOrManager)
            {
                if (isTechnician)
                {
                    q = q.Where(a =>
                        a.Audience == AnnouncementAudience.TechniciansOnly ||
                        a.Audience == AnnouncementAudience.EmployeesAndTechnicians);
                }
                else if (isEmployee)
                {
                    q = q.Where(a =>
                        a.Audience == AnnouncementAudience.EmployeesAndTechnicians);
                }
                else
                {
                    // دور غير معروف → لا شيء
                    q = q.Where(a => false);
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                q = q.Where(a =>
                    a.Title.Contains(search) ||
                    a.Content.Contains(search));
            }

            q = q.OrderByDescending(a => a.CreatedAt);

            var totalCount = await q.CountAsync(ct);

            var items = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            Func<string, string> urlBuilder = fn => _fileService.GetPublicUrl(fn);

            var dtoList = items
                .Select(a =>
                {
                    var creatorName = language.Equals("ar", StringComparison.OrdinalIgnoreCase)
                        ? a.CreatedByUser.FullNameAr ?? a.CreatedByUser.UserName
                        : a.CreatedByUser.FullNameEn ?? a.CreatedByUser.UserName;

                    return AnnouncementMapper.ToListItem(a, creatorName, urlBuilder);
                })
                .ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResultDTO<AnnouncementListItemDTO>
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Data = dtoList
            };
        }

        public async Task<AnnouncementDetailsDTO?> GetByIdForUserAsync(
            int id,
            string userId,
            string userRole,
            string language,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var q = _repo.Query(
                asTracking: false,
                include: a => a
                    .Include(x => x.Images)
                    .Include(x => x.CreatedByUser),
                predicate: a => a.Id == id);

            var entity = await q.FirstOrDefaultAsync(ct);
            if (entity is null) return null;

            // تأكد من الصلاحيات (نفس منطق GetForUserAsync)
            bool isAdminOrManager = CanManage(userRole);
            bool isTechnician = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            bool isEmployee = userRole.Equals("Employee", StringComparison.OrdinalIgnoreCase);

            if (!isAdminOrManager)
            {
                if (isTechnician)
                {
                    if (entity.Audience != AnnouncementAudience.TechniciansOnly &&
                        entity.Audience != AnnouncementAudience.EmployeesAndTechnicians)
                        return null;
                }
                else if (isEmployee)
                {
                    if (entity.Audience != AnnouncementAudience.EmployeesAndTechnicians)
                        return null;
                }
                else
                {
                    return null;
                }
            }

            var creatorName = language.Equals("ar", StringComparison.OrdinalIgnoreCase)
                ? entity.CreatedByUser.FullNameAr ?? entity.CreatedByUser.UserName
                : entity.CreatedByUser.FullNameEn ?? entity.CreatedByUser.UserName;

            Func<string, string> urlBuilder = fn => _fileService.GetPublicUrl(fn);

            return AnnouncementMapper.ToDetails(entity, creatorName, urlBuilder);
        }
    }
}
