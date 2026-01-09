using Azure.Core;
using Fixtroller.BLL.Helpers;
using Fixtroller.BLL.Mapping;
using Fixtroller.BLL.Services.FileService;
using Fixtroller.BLL.Services.GenericService;
using Fixtroller.BLL.Services.NotificationServices;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.PagedResultDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.MaintenanceRequestRepositories;
using Fixtroller.DAL.Repositories.UserRepository;
using Fixtroller.DAL.Repositories.UserRepository.TechnicianRepositorirs;
using Fixtroller.DAL.UnitOfWork;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.MaintenanceRequestServices
{
    public class MaintenanceRequestService : GenericService<MaintenanceRequestRequestDTO, MaintenanceRequestResponseDTO, MaintenanceRequest>, IMaintenanceRequestService
    {
        private readonly IMaintenanceRequestRepository _repository;
        private readonly ITechnicianRepository _techRepo;
        private readonly IFileService _fileService;
        private readonly IUnitOfWork _uow;
        private readonly IWorkTimeRepository _workRepo;
        private readonly IMaintenanceRequestTechnicianRepository _reqTechRepo;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepo;
        private readonly ILogger<MaintenanceRequestService> _logger;

        public MaintenanceRequestService(
            IMaintenanceRequestRepository repository,
            ITechnicianRepository techRepo,
            IFileService fileService,
            IUnitOfWork uow,
            IWorkTimeRepository workRepo,
            IMaintenanceRequestTechnicianRepository reqTechRepo,
            INotificationService notificationService,
            IUserRepository userRepo,
            ILogger<MaintenanceRequestService> logger
        ) : base(repository, uow)
        {
            _repository = repository;
            _techRepo = techRepo;
            _fileService = fileService;
            _uow = uow;
            _workRepo = workRepo;
            _reqTechRepo = reqTechRepo;
            _notificationService = notificationService;
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<int> CreateWithFile(
      MaintenanceRequestRequestDTO request,
      string userId,
      string language = "ar",
      CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            language = string.IsNullOrWhiteSpace(language) ? "ar" : language;

            // 1) تجهيز الكيان
            var entity = MaintenanceRequestMapper.ToEntity(
                request,
                ownerUserId: userId,
                createdByUserId: userId);

            // 2) رفع الصور (Sync + مع إمكانية التعويض عند الفشل)
            var uploaded = new List<string>();

            if (request.Images != null)
            {
                foreach (var f in request.Images)
                {
                    if (f != null && f.Length > 0)
                    {
                        var name = await _fileService.UploadAsync(f, ct);
                        uploaded.Add(name);
                    }
                }
            }

            for (int i = 0; i < uploaded.Count; i++)
            {
                entity.Images.Add(new MaintenanceRequestImage
                {
                    FileName = uploaded[i],
                    IsPrimary = (i == 0),
                    Source = MaintenanceRequestImageSource.RequestCreation
                });
            }

            if (entity.Images.Count > 0 && !entity.Images.Any(i => i.IsPrimary))
                entity.Images.First().IsPrimary = true;

            await _uow.BeginTransactionAsync(ct);

            try
            {
                // 3) حفظ الطلب وصوره في قاعدة البيانات
                await _repository.AddAsync(entity);
                await _uow.SaveAndCommitAsync(ct);

                var requestId = entity.Id;
                var notifLanguage = language;

                // 4) إرسال الإشعارات في الخلفية (non-blocking)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var recipients = new HashSet<string>(StringComparer.Ordinal);

                        await AddRoleRecipientsAsync(recipients, "MaintenanceManager", null, CancellationToken.None);
                        await AddRoleRecipientsAsync(recipients, "Admin", null, CancellationToken.None);

                        foreach (var uid in recipients)
                        {
                            await _notificationService.CreateAsync(new NotificationCreateModel
                            {
                                UserId = uid,
                                MaintenanceRequestId = requestId,
                                Type = NotificationType.RequestStatusChanged,
                                Severity = NotificationSeverity.Info,
                                Language = notifLanguage,
                                TitleKey = "NOTIF_REQUEST_CREATED_TITLE",
                                BodyKey = "NOTIF_REQUEST_CREATED_BODY",
                                BodyArgs = new object[] { requestId },
                                Channels = NotificationChannel.InApp | NotificationChannel.Email
                            }, CancellationToken.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Background notifications failed.");
                    }
                });

                _logger.LogInformation(
                    "Maintenance request created. RequestId={RequestId}, OwnerUserId={OwnerUserId}, CreatedByUserId={CreatedByUserId}, Priority={Priority}",
                    entity.Id,
                    entity.OwnerUserId,
                    entity.CreatedByUserId,
                    entity.Priority);

                return entity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating maintenance request. Rolling back transaction.");

                await _uow.RollbackAsync(ct);

                // حذف الملفات المرفوعة عند الفشل
                foreach (var name in uploaded)
                {
                    try
                    {
                        await _fileService.DeleteAsync(name, CancellationToken.None);
                    }
                    catch { /* تجاهل أي خطأ في الحذف */ }
                }

                throw;
            }
        }


        public async Task<(int? Id, string MessageKey)> CreateScenarioAsync(
       MaintenanceRequestScenarioRequestDTO request,
       string callerUserId,
       string callerRole,
       string language = "ar",
       CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            // 1) تأمين الدور
            var isManager = callerRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isAdmin = callerRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            var isTech = callerRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);

            if (!isManager && !isAdmin && !isTech)
            {
                // مفتاح موجود في الـ resources
                return (null, "Forbidden");
            }

            // 2) التحقق من الموظف صاحب الطلب
            if (string.IsNullOrWhiteSpace(request.OwnerUserId))
            {
                // رسالة عامة (موجودة في الـ resx)
                return (null, "BadRequest");
            }

            var newCase = request.CaseType;

            if (!IsValidCaseType(newCase))
                return (null, "CaseType_Invalid");

            if (!IsValidPriority(request.Priority))
                return (null, "Priority_Invalid");

            // 3) القواعد حسب ما طلبت انت 👇
            // الفني: Submitted, Processing, ManagerReview, ResourcesNeeded
            var techAllowed =
                newCase == CaseType.Submitted ||
                newCase == CaseType.Processing ||
                newCase == CaseType.ManagerReview ||
                newCase == CaseType.ResourcesNeeded;

            // المدير / الأدمن: Submitted, Processing, ResourcesNeeded, Completed
            var managerAllowed =
                newCase == CaseType.Submitted ||
                newCase == CaseType.Processing ||
                newCase == CaseType.ResourcesNeeded ||
                newCase == CaseType.Processed ||
                newCase == CaseType.Completed;

            if (isTech && !techAllowed)
            {
                return (null, "Case_NotAllowedForTechnician");
            }

            if ((isManager || isAdmin) && !managerAllowed)
            {
                return (null, "Case_NotAllowedForManager");
            }

            // 4) ننشئ الكيان كأن OwnerUserId هو اللي قدّم الطلب
            var entity = MaintenanceRequestMapper.ToEntity(
                request,
                ownerUserId: request.OwnerUserId,
                createdByUserId: callerUserId);

            // نغيّر الحالة الافتراضية (Submitted) إلى الحالة المختارة
            entity.CaseType = newCase;

            // في الحالات النهائية، نحدّث UpdatedAt
            if (entity.CaseType == CaseType.Processed || entity.CaseType == CaseType.Completed)
            {
                entity.UpdatedAt = DateTime.UtcNow;
                entity.ClosedAtUtc = DateTime.UtcNow;
            }

            // 5) رفع الملفات بنفس منطق CreateWithFile
            var uploaded = new List<string>();
            if (request.Images != null)
            {
                foreach (var f in request.Images)
                {
                    if (f != null && f.Length > 0)
                        uploaded.Add(await _fileService.UploadAsync(f, ct));
                }
            }

            for (int i = 0; i < uploaded.Count; i++)
            {
                entity.Images.Add(new MaintenanceRequestImage
                {
                    FileName = uploaded[i],
                    IsPrimary = (i == 0),
                    Source = MaintenanceRequestImageSource.RequestCreation
                });
            }

            if (entity.Images.Count > 0 && !entity.Images.Any(i => i.IsPrimary))
                entity.Images.First().IsPrimary = true;

            // 7) ترانزاكشن الحفظ
            await _uow.BeginTransactionAsync(ct);
            try
            {
                await _repository.AddAsync(entity);
                await _uow.SaveAndCommitAsync(ct);

                // نحفظ القيم اللي نحتاجها في الخلفية
                var requestId = entity.Id;
                var ownerUserId = entity.OwnerUserId;
                var notifLanguage = language;
                var callerRoleLocal = callerRole;

                // 🔔 الإشعارات في الخلفية (non-blocking)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // إشعار لصاحب الطلب بأن تم إنشاء طلب له
                        if (!string.IsNullOrWhiteSpace(ownerUserId))
                        {
                            await _notificationService.CreateAsync(new NotificationCreateModel
                            {
                                UserId = ownerUserId,
                                MaintenanceRequestId = requestId,
                                Type = NotificationType.RequestStatusChanged,
                                Severity = NotificationSeverity.Info,
                                Language = notifLanguage,

                                // ✅ localization
                                TitleKey = "NOTIF_REQUEST_CREATED_FOR_YOU_TITLE",
                                BodyKey = "NOTIF_REQUEST_CREATED_FOR_YOU_BODY",
                                BodyArgs = new object[] { requestId },

                                Channels = NotificationChannel.InApp | NotificationChannel.Email
                            }, CancellationToken.None);
                        }

                        // إشعارات لمدراء الصيانة عن طلب جديد
                        var recipients = new HashSet<string>(StringComparer.Ordinal);

                        await AddRoleRecipientsAsync(recipients, "MaintenanceManager", null, CancellationToken.None);
                        await AddRoleRecipientsAsync(recipients, "Admin", null, CancellationToken.None);

                        foreach (var uid in recipients)
                        {
                            await _notificationService.CreateAsync(new NotificationCreateModel
                            {
                                UserId = uid,
                                MaintenanceRequestId = requestId,
                                Type = NotificationType.RequestStatusChanged,
                                Severity = NotificationSeverity.Info,
                                Language = notifLanguage,
                                TitleKey = "NOTIF_REQUEST_CREATED_SCENARIO_TITLE",
                                BodyKey = "NOTIF_REQUEST_CREATED_SCENARIO_BODY",
                                BodyArgs = new object[] { requestId, callerRoleLocal },
                                Channels = NotificationChannel.InApp | NotificationChannel.Email
                            }, CancellationToken.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Background notifications failed for CreateScenarioAsync. RequestId={RequestId}",
                            requestId);
                    }
                });

                // نجاح: نرجّع Id + MessageKey جاهز للترجمة في الـ PL
                return (entity.Id, "Created"); // "Created" موجودة في SharedResource
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating scenario request. Rolling back transaction.");

                await _uow.RollbackAsync(ct);

                // تعويض ملفات الصور
                foreach (var name in uploaded)
                {
                    try { await _fileService.DeleteAsync(name, ct); } catch { }
                }

                // خطأ غير متوقّع
                throw;
            }
        }



        public async Task<PagedResultDTO<MaintenanceRequestListMineDTO>> GetMineAsync(
            string userId,
            string role,
            string language,
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            CaseType? caseType = null,
            int? requestId = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            language = string.IsNullOrWhiteSpace(language) ? "ar" : language;

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            static DateTime NormalizeEnd(DateTime d) =>
                d.TimeOfDay == TimeSpan.Zero ? d.Date.AddDays(1).AddTicks(-1) : d;

            if (createdFrom.HasValue && createdTo.HasValue && createdFrom.Value > createdTo.Value)
                (createdFrom, createdTo) = (createdTo, createdFrom);

            var end = createdTo.HasValue ? NormalizeEnd(createdTo.Value) : (DateTime?)null;

            IQueryable<MaintenanceRequest> q = _repository.Query(
                    asTracking: false,
                    predicate: x =>
                        x.Status == Status.Active &&
                        x.OwnerUserId == userId)
                .Include(x => x.ProblemType)
                    .ThenInclude(pt => pt.Translations);

            // ✅ بحث بالـ Id
            if (requestId.HasValue && requestId.Value > 0)
                q = q.Where(x => x.Id == requestId.Value);

            if (createdFrom.HasValue)
                q = q.Where(x => x.CreatedAt >= createdFrom.Value);

            if (end.HasValue)
                q = q.Where(x => x.CreatedAt <= end.Value);

            if (caseType.HasValue)
                q = q.Where(x => x.CaseType == caseType.Value);

            var totalCount = await q.CountAsync(ct);

            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedRows = await q
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    Light = new MaintenanceRequest
                    {
                        Id = x.Id,
                        Title = x.Title,
                        CaseType = x.CaseType,
                        Priority = x.Priority,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        CreatedByUserId = x.CreatedByUserId
                    },
                    ProblemTypeName = x.ProblemType != null
                        ? x.ProblemType.Translations
                            .OrderBy(t =>
                                t.Language == language ? 0 :
                                t.Language == "ar" ? 1 : 2)
                            .Select(t => t.Name)
                            .FirstOrDefault()
                        : null,
                    IsOwner = true
                })
                .ToListAsync(ct);

            var data = pagedRows
                .Select(r => MaintenanceRequestMapper.ToMineListItem(
                    r.Light, role, r.IsOwner, language, r.ProblemTypeName))
                .ToList();

            return new PagedResultDTO<MaintenanceRequestListMineDTO>
            {
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                TotalCount = totalCount,
                PageSize = pageSize,
                Data = data
            };
        }

        public async Task<PagedResultDTO<MaintenanceRequestListAllDTO>> GetAllAsync(
    string role,
    string language,
    DateTime? createdFrom = null,
    DateTime? createdTo = null,
    CaseType? caseType = null,
    int? requestId = null,
    int pageNumber = 1,
    int pageSize = 10,
    CancellationToken ct = default)
        {
            language = string.IsNullOrWhiteSpace(language) ? "ar" : language;

            var isManager = role.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            // لو مش مدير ولا أدمن، رجّع فاضي
            if (!isManager && !isAdmin)
            {
                return new PagedResultDTO<MaintenanceRequestListAllDTO>
                {
                    TotalPages = 0,
                    CurrentPage = 1,
                    TotalCount = 0,
                    PageSize = pageSize,
                    Data = new List<MaintenanceRequestListAllDTO>()
                };
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            // ✅ Normalize + تأمين نطاق التاريخ
            static DateTime NormalizeEnd(DateTime d) =>
                d.TimeOfDay == TimeSpan.Zero ? d.Date.AddDays(1).AddTicks(-1) : d;

            if (createdFrom.HasValue && createdTo.HasValue && createdFrom.Value > createdTo.Value)
                (createdFrom, createdTo) = (createdTo, createdFrom);

            var end = createdTo.HasValue ? NormalizeEnd(createdTo.Value) : (DateTime?)null;

            // 1) الكويري الأساسي مع Include للـ ProblemType + Translations
            IQueryable<MaintenanceRequest> q = _repository.Query(
                    asTracking: false,
                    predicate: x => x.Status == Status.Active)
                .Include(x => x.ProblemType)
                    .ThenInclude(pt => pt.Translations);

            // ✅ بحث بالـ Id
            if (requestId.HasValue && requestId.Value > 0)
                q = q.Where(x => x.Id == requestId.Value);

            // ✅ تطبيق الفلاتر (CreatedAt + CaseType)
            if (createdFrom.HasValue)
                q = q.Where(x => x.CreatedAt >= createdFrom.Value);

            if (end.HasValue)
                q = q.Where(x => x.CreatedAt <= end.Value);

            if (caseType.HasValue)
                q = q.Where(x => x.CaseType == caseType.Value);

            // 2) إجمالي السجلات
            var totalCount = await q.CountAsync(ct);

            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            // 3) الصفحة المطلوبة + اختيار أول فني نشط
            var pagedRows = await q
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    Light = new MaintenanceRequest
                    {
                        Id = x.Id,
                        Title = x.Title,
                        CaseType = x.CaseType,
                        Priority = x.Priority,
                        CreatedAt = x.CreatedAt
                    },
                    ProblemTypeName = x.ProblemType != null
                        ? x.ProblemType.Translations
                            .OrderBy(t =>
                                t.Language == language ? 0 :
                                t.Language == "ar" ? 1 : 2)
                            .Select(t => t.Name)
                            .FirstOrDefault()
                        : null,

                    // أول فني "نشط" (UnassignedAtUtc == null) حسب أقدم AssignedAtUtc
                    FirstTechnicianUserId = x.Technicians
                        .Where(t => t.UnassignedAtUtc == null)
                        .OrderBy(t => t.AssignedAtUtc)
                        .Select(t => t.TechnicianUserId)
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            // 4) جهّز أسماء الفنيين
            var techIds = pagedRows
                .Select(r => r.FirstTechnicianUserId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var techNames = new Dictionary<string, string>();

            foreach (var techId in techIds)
            {
                var user = await _userRepo.GetByIdAsync(techId!, ct);
                if (user is not null)
                {
                    techNames[techId!] = user.GetDisplayName(language);
                }
            }

            // 5) المابر إلى DTO مع تمرير الفني الأول (إن وجد)
            var data = pagedRows
                .Select(r =>
                {
                    string? techId = r.FirstTechnicianUserId;
                    string? techName = null;

                    if (!string.IsNullOrWhiteSpace(techId) &&
                        techNames.TryGetValue(techId, out var name))
                    {
                        techName = name;
                    }

                    return MaintenanceRequestMapper.ToAllListItem(
                        r.Light,
                        r.ProblemTypeName,
                        language,
                        technicianUserId: techId,
                        technicianName: techName
                    );
                })
                .ToList();

            // 6) النتيجة
            return new PagedResultDTO<MaintenanceRequestListAllDTO>
            {
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                TotalCount = totalCount,
                PageSize = pageSize,
                Data = data
            };
        }




        public async Task<MaintenanceRequestResponseDTO?> GetByIdAsync(
            int id,
            string userId,
            string role,
            string language = "ar",
            CancellationToken ct = default)
        {
            // ثوابت تُترجم لـ SQL
            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            var isManager = string.Equals(role, "MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isEmployee = string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase);
            var isTechnician = string.Equals(role, "Technician", StringComparison.OrdinalIgnoreCase);

            var e = await _repository.Query(
                        asTracking: false,
                        predicate: x =>
                            x.Id == id &&
                            (
                                isAdmin ||
                                isManager ||
                                (isEmployee && x.OwnerUserId == userId) ||
                                (isTechnician && (x.CreatedByUserId == userId ||
                                                  x.Technicians.Any(t => t.UnassignedAtUtc == null && t.TechnicianUserId == userId)))
                            ),
                        include: q => q
                             .Include(r => r.OwnerUser)
                            .Include(r => r.Images)
                            .Include(r => r.Notes)
                            .ThenInclude(n => n.CreatedByUser)
                            .Include(r => r.Technicians.Where(t => t.UnassignedAtUtc == null))
                             .Include(r => r.ProblemType)
                                .ThenInclude(pt => pt.Translations)
                    )
                    .FirstOrDefaultAsync(ct);

            if (e is null) return null;

            var isOwner = string.Equals(e.OwnerUserId, userId, StringComparison.Ordinal);


            var dto = MaintenanceRequestMapper.ToResponse(e, role, _fileService.GetPublicUrl, language, isOwner, includeOwnerDetails: isAdmin || isManager || isTechnician);

            // لو المستخدم الحالي فني: نبحث عن مؤقّت عمل نشط له على هذا الطلب
            if (isTechnician)
            {
                var activeEntry = await _workRepo.Query(asTracking: false)
                    .Where(w => w.RequestId == e.Id &&
                                w.TechnicianUserId == userId &&
                                w.StoppedAt == null)
                    .OrderByDescending(w => w.StartedAt)
                    .FirstOrDefaultAsync(ct);

                if (activeEntry is not null)
                {
                    var now = DateTimeOffset.UtcNow;
                    var seconds = (int)Math.Max(0, (now - activeEntry.StartedAt).TotalSeconds);

                    dto.CurrentTechnicianActiveSeconds = seconds;
                }
            }
            await EnrichAssignedTechniciansNamesAsync(dto, language, ct);

            return dto;
        }

        public async Task<(int? RequestId, string MessageKey)> AssignTechniciansAsync(
        int requestId,
        IEnumerable<string> technicianUserIds,
        int? expectedDuration,
        string language = "ar",
        CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var request = await _repository.GetForAssignmentAsync(requestId, ct);
            if (request is null) return (null, "Request_NotFound");

            var list = (technicianUserIds ?? Enumerable.Empty<string>())
                       .Where(s => !string.IsNullOrWhiteSpace(s))
                       .Select(s => s.Trim())
                       .Distinct(StringComparer.Ordinal)
                       .ToList();

            if (list.Count == 0)
                return (null, "Technician_ListEmpty");

            // تحقق من صحة كل معرّف ودوره
            foreach (var tid in list)
            {
                var tech = await _techRepo.GetByIdAsync(tid, ct);
                if (tech is null) return (null, "Technician_NotFound");

                var isTechnician = await _techRepo.IsInRoleAsync(tid, "Technician", ct);
                if (!isTechnician) return (null, "User_NotTechnician");

                var nowUtc = DateTimeOffset.UtcNow;
                if (tech.LockoutEnd.HasValue && tech.LockoutEnd > nowUtc)
                {
                    return (null, "Technician_IsOnVacation");
                }
            }

            await _uow.BeginTransactionAsync(ct);
            try
            {
                // الفنيين الحاليين
                var current = await _reqTechRepo.GetActiveTechniciansAsync(requestId, ct);

                // الفنيين الجدد فقط (اللي مش موجودين أصلاً)
                var added = list.Except(current, StringComparer.Ordinal).ToList();

                // نعمل اتحاد للقوائم: الحالي + الجدد
                var newActiveList = current
                    .Concat(added)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                // مزامنة التعيينات بدون شطب السابقين
                await _reqTechRepo.SetActiveListAsync(requestId, newActiveList, expectedDuration, ct);

                // لو Submitted ارفعها إلى Processing
                if (request.CaseType == CaseType.Submitted)
                    request.CaseType = CaseType.Processing;

                request.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveAndCommitAsync(ct);

                // نحضّر قيم للإشعارات في الخلفية
                var addedLocal = added.ToList();
                var reqId = request.Id;
                var notifLanguage = language;

                // إشعارات للفنيين الجدد فقط (في الخلفية - non-blocking)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        foreach (var tid in addedLocal)
                        {
                            await _notificationService.CreateAsync(new NotificationCreateModel
                            {
                                UserId = tid,
                                MaintenanceRequestId = reqId,
                                Type = NotificationType.RequestAssigned,
                                Severity = NotificationSeverity.Info,
                                Language = notifLanguage,
                                // ✅ localization
                                TitleKey = "NOTIF_ASSIGNED_TITLE",
                                BodyKey = "NOTIF_ASSIGNED_BODY",
                                BodyArgs = new object[] { reqId },

                                Channels = NotificationChannel.InApp | NotificationChannel.Email
                            }, CancellationToken.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Background notifications failed for AssignTechniciansAsync. RequestId={RequestId}",
                            reqId);
                    }
                });

                return (request.Id, "Technicians_Assigned");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while assigning technicians. RequestId={RequestId}",
                    requestId);

                await _uow.RollbackAsync(ct);
                throw;
            }
        }


        public async Task<(int? RequestId, string MessageKey)> AssignTechnicianAsync(
      int requestId,
      string technicianUserId,
      int? expectedDuration,
      string language = "ar",
      CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var request = await _repository.GetForAssignmentAsync(requestId, ct);
            if (request is null) return (null, "Request_NotFound");

            var tech = await _techRepo.GetByIdAsync(technicianUserId, ct);
            if (tech is null) return (null, "Technician_NotFound");

            var isTechnician = await _techRepo.IsInRoleAsync(technicianUserId, "Technician", ct);
            if (!isTechnician) return (null, "User_NotTechnician");

            var nowUtc = DateTimeOffset.UtcNow;
            if (tech.LockoutEnd.HasValue && tech.LockoutEnd > nowUtc)
            {
                return (null, "Technician_IsOnVacation");
            }

            // إن كان مُعيَّن نشطًا أصلًا، لا تغيّر شيء
            var already = await _reqTechRepo.IsActiveAssignedAsync(requestId, technicianUserId, ct);
            if (already)
            {
                return (requestId, "Technician_AlreadyAssigned");
            }

            _logger.LogInformation(
                "Assigning technician {TechnicianUserId} to request {RequestId}",
                technicianUserId,
                requestId);

            await _uow.BeginTransactionAsync(ct);
            try
            {
                await _reqTechRepo.AddActiveAsync(requestId, technicianUserId, expectedDuration, ct);

                if (request.CaseType == CaseType.Submitted)
                    request.CaseType = CaseType.Processing;

                request.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveAndCommitAsync(ct);

                // نحضّر قيم الإشعار للخلفية
                var reqId = request.Id;
                var techIdLocal = technicianUserId;
                var notifLanguage = language;

                // إشعار للفني المعيّن (في الخلفية - non-blocking)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _notificationService.CreateAsync(new NotificationCreateModel
                        {
                            UserId = techIdLocal,
                            MaintenanceRequestId = reqId,
                            Type = NotificationType.RequestAssigned,
                            Severity = NotificationSeverity.Info,
                            Language = notifLanguage,
                            // ✅ localization
                            TitleKey = "NOTIF_ASSIGNED_TITLE",
                            BodyKey = "NOTIF_ASSIGNED_BODY",
                            BodyArgs = new object[] { reqId },

                            Channels = NotificationChannel.InApp | NotificationChannel.Email
                        }, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Background notification failed for AssignTechnicianAsync. RequestId={RequestId}, TechnicianUserId={TechnicianUserId}",
                            reqId,
                            techIdLocal);
                    }
                });

                _logger.LogInformation(
                    "Technician {TechnicianUserId} assigned to request {RequestId} successfully",
                    technicianUserId,
                    request.Id);

                return (request.Id, "Technician_Assigned");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while assigning technician. RequestId={RequestId}, TechnicianUserId={TechnicianUserId}",
                    requestId,
                    technicianUserId);

                await _uow.RollbackAsync(ct);
                throw;
            }
        }


        public async Task<(bool ok, string messageKey)> RemoveTechnicianAsync(
     int requestId,
     string technicianUserId,
     string language = "ar",
     CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            // 1) تأكد الطلب موجود
            var r = await _repository.GetForUpdateAsync(requestId, ct);
            if (r is null) return (false, "Request_NotFound");

            // 2) تأكد أن الفني مُعيَّن نشطًا أصلًا
            var isActive = await _reqTechRepo.IsActiveAssignedAsync(requestId, technicianUserId, ct);
            if (!isActive) return (false, "Technician_NotActiveOnRequest");

            await _uow.BeginTransactionAsync(ct);
            try
            {
                // 3) شطب التعيين النشط
                await _reqTechRepo.RemoveActiveAsync(requestId, technicianUserId, ct);

                // 4) إيقاف أي مؤقت نشط لهذا الفني على هذا الطلب
                await _workRepo.StopActiveForRequestAndTechAsync(requestId, technicianUserId, ct);

                // 5) تحديث طابع الوقت
                r.UpdatedAt = DateTime.UtcNow;

                // 6) حفظ + Commit
                await _uow.SaveAndCommitAsync(ct);

                // نحضّر قيم الإشعارات للخلفية
                var reqId = r.Id;
                var techIdLocal = technicianUserId;
                var notifLanguage = language;

                // 🔔 الإشعارات في الخلفية (non-blocking)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // إشعار للفني الذي تمّت إزالته
                        await _notificationService.CreateAsync(new NotificationCreateModel
                        {
                            UserId = techIdLocal,
                            MaintenanceRequestId = reqId,
                            Type = NotificationType.RequestStatusChanged,
                            Severity = NotificationSeverity.Info,
                            Language = notifLanguage,
                            // ✅ localization
                            TitleKey = "NOTIF_REMOVED_FROM_REQUEST_TITLE",
                            BodyKey = "NOTIF_REMOVED_FROM_REQUEST_BODY",
                            BodyArgs = new object[] { reqId },

                            Channels = NotificationChannel.InApp | NotificationChannel.Email
                        }, CancellationToken.None);

                        // إشعارات لمدراء الصيانة
                        var recipients = new HashSet<string>(StringComparer.Ordinal);

                        await AddRoleRecipientsAsync(recipients, "MaintenanceManager", null, CancellationToken.None);
                        await AddRoleRecipientsAsync(recipients, "Admin", null, CancellationToken.None);

                        foreach (var uid in recipients)
                        {
                            await _notificationService.CreateAsync(new NotificationCreateModel
                            {
                                UserId = uid,
                                MaintenanceRequestId = reqId,
                                Type = NotificationType.RequestStatusChanged,
                                Severity = NotificationSeverity.Info,
                                Language = notifLanguage,
                                TitleKey = "NOTIF_TECH_REMOVED_TITLE",
                                BodyKey = "NOTIF_TECH_REMOVED_BODY",
                                BodyArgs = new object[] { reqId },
                                Channels = NotificationChannel.InApp | NotificationChannel.Email
                            }, CancellationToken.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Background notifications failed for RemoveTechnicianAsync. RequestId={RequestId}, TechnicianUserId={TechnicianUserId}",
                            reqId,
                            techIdLocal);
                    }
                });

                return (true, "Technician_Removed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while removing technician from request. RequestId={RequestId}, TechnicianUserId={TechnicianUserId}",
                    requestId,
                    technicianUserId);

                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<(bool ok, string messageKey)> StartWorkAsync(
       int requestId,
       string technicianUserId,
       string callerUserId,
       string callerRole,
       string language = "ar",
       CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            // 1) تحقق الطلب
            var req = await _repository.GetForUpdateAsync(requestId, ct);
            if (req is null) return (false, "Request_NotFound");

            // 2) الصلاحيات:
            // - الفني: لازم يبدأ لنفسه (callerUserId == technicianUserId)
            // - المدير: يقدر يبدأ لأي فني مُعيَّن نشطًا
            var isManager = callerRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isTech = callerRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            var callerIsSameTech = string.Equals(callerUserId, technicianUserId, StringComparison.Ordinal);

            if (!(isManager || (isTech && callerIsSameTech)))
                return (false, "Forbidden");

            // 3) تأكد أن الفني المطلوب مُعيَّن نشطًا على الطلب
            var isAssignedActive = await _reqTechRepo.IsActiveAssignedAsync(requestId, technicianUserId, ct);
            if (!isAssignedActive) return (false, "Request_NotAssignedToThisTechnician");

            // 4) منع الازدواج
            var hasActive = await _workRepo.HasActiveAsync(requestId, technicianUserId, ct);
            if (hasActive) return (false, "Work_AlreadyStarted");

            // 5) ابدأ ضمن ترانزاكشن
            await _uow.BeginTransactionAsync(ct);
            try
            {
                await _workRepo.StartAsync(new WorkTimeEntry
                {
                    RequestId = requestId,
                    TechnicianUserId = technicianUserId,
                    StartedAt = DateTimeOffset.UtcNow
                }, ct);

                await _uow.SaveAndCommitAsync(ct);

                // نحضّر قيم الإشعار للخلفية
                var reqId = req.Id;
                var techIdLocal = technicianUserId;
                var notifLang = language;

                // ( إشعار للفني ) في الخلفية - non-blocking
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _notificationService.CreateAsync(new NotificationCreateModel
                        {
                            UserId = techIdLocal,
                            MaintenanceRequestId = reqId,
                            Type = NotificationType.RequestStatusChanged,
                            Severity = NotificationSeverity.Info,
                            Language = notifLang,
                            // ✅ localization
                            TitleKey = "NOTIF_WORK_STARTED_TITLE",
                            BodyKey = "NOTIF_WORK_STARTED_BODY",
                            BodyArgs = new object[] { reqId },

                            Channels = NotificationChannel.InApp | NotificationChannel.Email
                        }, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Background notification failed for StartWorkAsync. RequestId={RequestId}, TechnicianUserId={TechnicianUserId}",
                            reqId,
                            techIdLocal);
                    }
                });

                return (true, "Work_Started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while starting work. RequestId={RequestId}, TechnicianUserId={TechnicianUserId}",
                    requestId,
                    technicianUserId);

                await _uow.RollbackAsync(ct);
                throw;
            }
        }


        public async Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> ChangeCaseAsync(
         int requestId,
         ChangeCaseTypeRequestDTO dto,
         string userId,
         string userRole,
         bool preferOwnerPath,
         string language = "ar",
         CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (!IsValidCaseType(dto.NewCaseType))
                return (null, "CaseType_Invalid");

            if (dto.Priority.HasValue && !IsValidPriority(dto.Priority.Value))
                return (null, "Priority_Invalid");

            if (dto.NoteType.HasValue && !IsValidNoteType(dto.NoteType.Value))
                return (null, "NoteType_Invalid");

            // تحقّقات بدون ترانزاكشن
            var r = await _repository.GetForUpdateAsync(requestId, ct);
            if (r is null) return (null, "Request_NotFound");

            var newCase = dto.NewCaseType;
            var oldCase = r.CaseType;

            if (!IsValidTransition(oldCase, newCase))
                return (null, "Case_InvalidTransition");

            bool isManager = userRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            bool isAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            bool isTechnician = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            bool isOwner = r.OwnerUserId == userId;

            var useOwnerPath = isOwner && (preferOwnerPath || !(isManager || isAdmin || isTechnician));
            var author = InferAuthor(isOwner, isTechnician, isManager, isAdmin);

            if (r.CaseType == newCase)
            {
                var fresh = await GetByIdAsync(requestId, userId, userRole, language, ct);
                return (fresh, "Case_NoChange");
            }

            // الحالات اللي بتتغيّر داخلياً فقط
            if (newCase is CaseType.Submitted or CaseType.Modified)
                return (null, "Case_AutoManaged");

            bool needNote = newCase is CaseType.Reopened or CaseType.ResourcesNeeded;
            if (needNote && string.IsNullOrWhiteSpace(dto.NoteText))
                return (null, "Note_Required_For_This_Case");

            if (newCase == CaseType.Reopened && !dto.Priority.HasValue)
                return (null, "Priority_Required_For_Reopen");

            var inferredType = newCase switch
            {
                CaseType.Reopened => NoteType.ReopenReason,
                CaseType.ResourcesNeeded => NoteType.HelpRequest,
                _ => dto.NoteType ?? NoteType.General
            };

            await _uow.BeginTransactionAsync(ct);
            try
            {
                // ===================== مسار المالك =====================
                if (useOwnerPath)
                {
                    var allowedOwner = new[] { CaseType.Cancelled, CaseType.Reopened, CaseType.Completed };
                    if (!allowedOwner.Contains(newCase))
                        return (null, "Case_NotAllowedForOwner");

                    r.CaseType = newCase;

                    if (newCase == CaseType.Reopened && dto.Priority.HasValue)
                        r.Priority = dto.Priority.Value;

                    if (needNote || !string.IsNullOrWhiteSpace(dto.NoteText))
                        r.Notes.Add(MaintenanceRequestMapper.ToNote(dto.NoteText!, inferredType, author, userId, r.Id));

                    if (newCase == CaseType.Reopened)
                    {
                        r.ClosedAtUtc = null;
                    }
                    else if (newCase == CaseType.Completed || newCase == CaseType.Cancelled)
                    {
                        r.ClosedAtUtc = DateTime.UtcNow;
                    }

                    r.UpdatedAt = DateTime.UtcNow;

                    // أوقف كل المؤقتات
                    await _workRepo.StopActiveForRequestAsync(requestId, ct);

                    if (newCase == CaseType.Completed || newCase == CaseType.Cancelled)
                    {
                        var activeTechs = await _reqTechRepo.GetActiveTechniciansAsync(requestId, ct);
                        foreach (var tid in activeTechs)
                        {
                            await _reqTechRepo.RemoveActiveAsync(requestId, tid, ct);
                        }
                    }

                    await _uow.SaveAndCommitAsync(ct);

                    // إشعار تغيير الحالة في الخلفية (مسار المالك)
                    var reqIdOwner = r.Id;
                    var newCaseOwner = newCase;
                    var langOwner = language;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SendStatusChangeNotificationAsync(r, newCaseOwner, langOwner, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Background status change notification failed (owner path). RequestId={RequestId}, NewCase={NewCase}",
                                reqIdOwner,
                                newCaseOwner);
                        }
                    });

                    var fresh2 = await GetByIdAsync(requestId, userId, userRole, language, ct);
                    return (fresh2, "Case_Changed");
                }

                // ===================== مسار الفني =====================
                if (isTechnician)
                {
                    var isActiveAssigned = await _reqTechRepo.IsActiveAssignedAsync(requestId, userId, ct);
                    if (!isActiveAssigned)
                        return (null, "Request_NotAssignedToYou");

                    var allowedTech = new[] { CaseType.ResourcesNeeded, CaseType.ManagerReview };
                    if (!allowedTech.Contains(newCase))
                        return (null, "Case_NotAllowedForTechnician");

                    r.CaseType = newCase;
                    if (needNote || !string.IsNullOrWhiteSpace(dto.NoteText))
                        r.Notes.Add(MaintenanceRequestMapper.ToNote(dto.NoteText!, inferredType, author, userId, r.Id));

                    r.UpdatedAt = DateTime.UtcNow;

                    // الفني لما يغيّر الحالة نوقف أي مؤقت شغال
                    await _workRepo.StopActiveForRequestAsync(requestId, ct);

                    await _uow.SaveAndCommitAsync(ct);

                    // إشعار تغيير الحالة في الخلفية (مسار الفني)
                    var reqIdTech = r.Id;
                    var newCaseTech = newCase;
                    var langTech = language;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SendStatusChangeNotificationAsync(r, newCaseTech, langTech, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Background status change notification failed (technician path). RequestId={RequestId}, NewCase={NewCase}",
                                reqIdTech,
                                newCaseTech);
                        }
                    });

                    var fresh1 = await GetByIdAsync(requestId, userId, userRole, language, ct);
                    return (fresh1, "Case_Changed");
                }

                // ===================== مسار المدير / الآدمن =====================
                if (!(isManager || isAdmin))
                    return (null, "Forbidden");

                r.CaseType = newCase;
                if (needNote || !string.IsNullOrWhiteSpace(dto.NoteText))
                    r.Notes.Add(MaintenanceRequestMapper.ToNote(dto.NoteText!, inferredType, author, userId, r.Id));

                if (newCase == CaseType.Reopened)
                {
                    r.ClosedAtUtc = null;
                }
                else if (newCase == CaseType.Completed || newCase == CaseType.Cancelled)
                {
                    r.ClosedAtUtc = DateTime.UtcNow;
                }

                r.UpdatedAt = DateTime.UtcNow;

                if (newCase == CaseType.Processing)
                {
                    var techs = await _reqTechRepo.GetActiveTechniciansAsync(requestId, ct);
                    if (techs.Count == 0)
                    {
                        await _uow.RollbackAsync(ct);
                        return (null, "Technician_NotAssigned");
                    }
                }
                else
                {
                    // باقي الحالات نوقف كل المؤقتات
                    await _workRepo.StopActiveForRequestAsync(requestId, ct);

                    // وفي حالة الإكمال / الإلغاء نشطب الربط مع الفنيين الفعّالين
                    if (newCase == CaseType.Completed || newCase == CaseType.Cancelled)
                    {
                        var activeTechs = await _reqTechRepo.GetActiveTechniciansAsync(requestId, ct);

                        foreach (var tid in activeTechs)
                            await _reqTechRepo.RemoveActiveAsync(requestId, tid, ct);
                    }
                    // ملاحظة: حالة Processed تدخل هنا → نوقف المؤقتات فقط بدون شطب الفنيين
                }

                await _uow.SaveAndCommitAsync(ct);

                // إشعار تغيير الحالة في الخلفية (مسار المدير/الآدمن)
                var reqIdMgr = r.Id;
                var newCaseMgr = newCase;
                var langMgr = language;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendStatusChangeNotificationAsync(r, newCaseMgr, langMgr, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Background status change notification failed (manager/admin path). RequestId={RequestId}, NewCase={NewCase}",
                            reqIdMgr,
                            newCaseMgr);
                    }
                });

                var fresh = await GetByIdAsync(requestId, userId, userRole, language, ct);

                _logger.LogInformation(
                    "Request {RequestId} case changed from {OldCase} to {NewCase}",
                    r.Id,
                    oldCase,
                    r.CaseType);

                return (fresh, "Case_Changed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while changing case for request {RequestId}. OldCase={OldCase}, NewCase={NewCase}",
                    requestId,
                    oldCase,
                    dto.NewCaseType);

                await _uow.RollbackAsync(ct);
                throw;
            }
        }


        public async Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> AddNoteAsync(
        int requestId,
        string userId,
        string userRole,
        AddNoteRequestDTO dto,
        string language = "ar",
        CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            // تحقّقات بدون ترانزاكشن
            var r = await _repository.GetForUpdateAsync(requestId, ct);
            if (r is null) return (null, "Request_NotFound");

            var isOwner = string.Equals(r.OwnerUserId, userId, StringComparison.Ordinal);
            var isTech = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            var isMgr = userRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            var author = InferAuthor(isOwner, isTech, isMgr, isAdmin);

            var lockedForManager = r.CaseType == CaseType.Completed || r.CaseType == CaseType.Cancelled;
            if (isMgr && lockedForManager && !isAdmin)
                return (null, "Notes_Disabled_For_Manager_In_FinalState");

            if (isTech)
            {
                var activeAssigned = await _reqTechRepo.IsActiveAssignedAsync(requestId, userId, ct);
                if (!activeAssigned) return (null, "Request_NotAssignedToYou");
            }

            if (!isOwner && !isTech && !isMgr && !isAdmin)
                return (null, "Forbidden");

            if (string.IsNullOrWhiteSpace(dto.Text))
                return (null, "Note_Text_Required");

            if (dto.Type.HasValue && !IsValidNoteType(dto.Type.Value))
                return (null, "NoteType_Invalid");

            var noteType = NoteType.General;

            await _uow.BeginTransactionAsync(ct);
            try
            {
                r.Notes.Add(MaintenanceRequestMapper.ToNote(dto.Text!, noteType, author, userId, r.Id));
                r.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveAndCommitAsync(ct);

                // 🔔 إشعار بإضافة ملاحظة جديدة على الطلب - في الخلفية (non-blocking)
                var reqId = r.Id;
                var noteText = dto.Text!;
                var ntType = noteType;
                var notifLang = language;
                var authorId = userId;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendNoteAddedNotificationAsync(r, ntType, authorId, noteText, notifLang, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Background note-added notification failed. RequestId={RequestId}, UserId={UserId}",
                            reqId,
                            authorId);
                    }
                });

                var fresh = await GetByIdAsync(requestId, userId, userRole, language, ct);
                return (fresh, "Note_Added");
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)>
            UpdateMineAsync(
                int id,
                string userId,
                string role,
                MaintenanceRequestUpdateDTO dto,
                string language = "ar",
                CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            // 0) جهّز عمليات الملفات
            var uploadedNewFiles = new List<string>();   // نعوّضها لو فشلنا
            var toDeleteFiles = new List<string>();      // نحذفها بعد الـ Commit

            // 1) ارفع الصور الجديدة خارج الترانزاكشن (تعويض عند الفشل)
            if (dto.NewImages != null && dto.NewImages.Count > 0)
            {
                foreach (var f in dto.NewImages)
                {
                    if (f is { Length: > 0 })
                    {
                        var name = await _fileService.UploadAsync(f, ct);
                        uploadedNewFiles.Add(name);
                    }
                }
            }

            await _uow.BeginTransactionAsync(ct);
            try
            {
                var r = await _repository.GetForUpdateAsync(id, ct);
                if (r is null) return (null, "Request_NotFound");

                // المالك فقط
                if (!string.Equals(r.OwnerUserId, userId, StringComparison.Ordinal))
                    throw new UnauthorizedAccessException("Forbidden");

                // الحالات المسموح فيها التعديل
                var editable = new HashSet<CaseType>
        {
            CaseType.Submitted,
            CaseType.Reopened,
            CaseType.Modified
        };
                if (!editable.Contains(r.CaseType))
                    return (null, "Request_NotEditableInThisState");

                // 2) تعديل الحقول النصية
                if (!string.IsNullOrWhiteSpace(dto.Title)) r.Title = dto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Description)) r.Description = dto.Description.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Address)) r.Address = dto.Address.Trim();
                if (dto.Priority.HasValue && !IsValidPriority(dto.Priority.Value))
                    return (null, "Priority_Invalid");
                if (dto.Priority.HasValue) r.Priority = dto.Priority.Value;
                if (dto.ProblemTypeId.HasValue) r.ProblemTypeId = dto.ProblemTypeId.Value;

                // 3) حذف صور (من الداتابيس الآن، ومن التخزين بعد الـ Commit)
                if (dto.RemoveImageIds is { Count: > 0 } && r.Images.Count > 0)
                {
                    var toRemove = r.Images.Where(i => dto.RemoveImageIds.Contains(i.Id)).ToList();
                    foreach (var img in toRemove)
                    {
                        toDeleteFiles.Add(img.FileName); // احذف فعليًا بعد الـ Commit
                        r.Images.Remove(img);
                    }
                }

                // 4) إضافة الصور المرفوعة حديثًا إلى الداتابيس
                foreach (var name in uploadedNewFiles)
                {
                    r.Images.Add(new MaintenanceRequestImage
                    {
                        FileName = name,
                        IsPrimary = false,
                        Source = MaintenanceRequestImageSource.RequestCreation
                    });
                }

                // 5) ضمان وجود صورة أساسية واحدة
                if (r.Images.Count > 0 && !r.Images.Any(i => i.IsPrimary))
                    r.Images.First().IsPrimary = true;

                // 6) انتقال الحالة وتحديث الوقت
                r.CaseType = CaseType.Modified;
                r.UpdatedAt = DateTime.UtcNow;

                // 7) حفظ + Commit مرّة واحدة
                await _uow.SaveAndCommitAsync(ct);

                // 8) بعد الـ Commit: نفّذ الحذف الفعلي للملفات التي أزلنا روابطها
                foreach (var filename in toDeleteFiles)
                {
                    try { await _fileService.DeleteAsync(filename, ct); } catch { /* تجاهل */ }
                }

                // 9) إشعارات للمديرين والفنيين أن صاحب الطلب عدّل الطلب 👇 (في الخلفية)
                var reqId = r.Id;
                var notifLang = language;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendRequestUpdatedByOwnerNotificationAsync(r, notifLang, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Background notification failed for UpdateMineAsync. RequestId={RequestId}",
                            reqId);
                    }
                });

                var isOwner = true; // مؤكّد من الفحص أعلاه

                var fresh = await GetByIdAsync(id, userId, role, language, ct);
                return (fresh, "Request_Updated");
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                foreach (var name in uploadedNewFiles)
                {
                    try { await _fileService.DeleteAsync(name, ct); } catch { /* ولا يهمك */ }
                }
                throw;
            }
        }








        public async Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> AddImagesAsync(
    int requestId,
    string userId,
    string userRole,
    AddImagesRequestDTO dto,
    string language = "ar",
    CancellationToken ct = default)
        {
            // 1) تحقّقات أولية
            var r = await _repository.GetForUpdateAsync(requestId, ct);
            if (r is null) return (null, "Request_NotFound");

            var isOwner = string.Equals(r.OwnerUserId, userId, StringComparison.Ordinal);
            var isTech = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            var isMgr = userRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            // نمنع إضافة صور بعد الإنهاء/الإلغاء (يسمح فقط للأدمن لو أردت)
            var lockedFinal = r.CaseType == CaseType.Completed || r.CaseType == CaseType.Cancelled;
            if (lockedFinal && !isAdmin)
                return (null, "Images_Disabled_In_FinalState");

            // الفني يجب أن يكون مُعيَّن تعيينًا نشطًا
            if (isTech)
            {
                var activeAssigned = await _reqTechRepo.IsActiveAssignedAsync(requestId, userId, ct);
                if (!activeAssigned) return (null, "Request_NotAssignedToYou");
            }

            // السماح: صاحب الطلب، الفني (لو مُعيّن)، المدير، الأدمن
            if (!isOwner && !isTech && !isMgr && !isAdmin)
                return (null, "Forbidden");

            // 2) تحقق من وجود ملفات
            if (dto?.Images is null || dto.Images.Count == 0)
                return (null, "Images_Empty");

            // (اختياري) تحقق النوع/الحجم
            // مثال بسيط: رفض > 10MB للصورة الواحدة
            const long maxSize = 10 * 1024 * 1024;
            foreach (var f in dto.Images)
            {
                if (f == null || f.Length <= 0) return (null, "Images_InvalidFile");
                if (f.Length > maxSize) return (null, "Images_TooLarge");
                // بإمكانك فحص ContentType إن رغبت: image/jpeg, image/png ...
            }

            // 3) ارفع وألصق
            var uploaded = new List<string>();

            await _uow.BeginTransactionAsync(ct);
            try
            {
                // ارفع أولًا
                foreach (var file in dto.Images)
                    uploaded.Add(await _fileService.UploadAsync(file, ct));

                // اربط بالطلب
                var hadPrimaryBefore = r.Images?.Any(i => i.IsPrimary) == true;
                for (int i = 0; i < uploaded.Count; i++)
                {
                    var isPrimary = false;
                    if (!hadPrimaryBefore && dto.MakePrimaryFirst && i == 0)
                        isPrimary = true;

                    r.Images.Add(new MaintenanceRequestImage
                    {
                        FileName = uploaded[i],
                        IsPrimary = isPrimary,
                        Source = MaintenanceRequestImageSource.StaffAttachment
                    });
                }

                r.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveAndCommitAsync(ct);
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                // نظّف الصور المرفوعة إذا فشل الحفظ
                foreach (var name in uploaded)
                {
                    try { await _fileService.DeleteAsync(name, ct); } catch { }
                }
                throw;
            }

            // 4) رجّع الردّ مع الروابط العامة
            var withIncludes = await _repository.Query(
                    asTracking: false,
                    include: q => q
                        .Include(x => x.Images)
                        .Include(x => x.Notes)
                        .Include(x => x.ProblemType).ThenInclude(pt => pt.Translations),
                    predicate: x => x.Id == requestId)
                .FirstOrDefaultAsync(ct);
            var fresh = await GetByIdAsync(requestId, userId, userRole, language, ct);
            return (fresh, "Images_Added");
        }


        public async Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> RemoveStaffImagesAsync(
    int requestId,
    string userId,
    string userRole,
    RemoveStaffImagesRequestDTO dto,
    string language = "ar",
    CancellationToken ct = default)
        {
            // 1) تحقّقات أولية
            var r = await _repository.GetForUpdateAsync(requestId, ct);
            if (r is null) return (null, "Request_NotFound");

            var isTech = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            var isMgr = userRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            // فقط: فني, مدير صيانة, أدمن
            if (!isTech && !isMgr && !isAdmin)
                return (null, "Forbidden");

            // نفس منطق AddImagesAsync: لا تعديل بعد الإنهاء/الإلغاء (إلا للأدمن)
            var lockedFinal = r.CaseType == CaseType.Completed || r.CaseType == CaseType.Cancelled;
            if (lockedFinal && !isAdmin)
                return (null, "Images_Disabled_In_FinalState");

            // الفني لازم يكون معيَّن على الطلب
            if (isTech)
            {
                var activeAssigned = await _reqTechRepo.IsActiveAssignedAsync(requestId, userId, ct);
                if (!activeAssigned) return (null, "Request_NotAssignedToYou");
            }

            if (dto?.ImageIds is null || dto.ImageIds.Count == 0)
                return (null, "Images_Empty");

            // 2) اختر فقط الصور التي أضيفت من AddImagesAsync
            var toRemove = r.Images
                .Where(i =>
                    dto.ImageIds.Contains(i.Id) &&
                    i.Source == MaintenanceRequestImageSource.StaffAttachment)
                .ToList();

            if (toRemove.Count == 0)
            {
                // ما في صور مطابقة للحذف
                return (null, "Images_NotFound_Or_NotStaff");
            }

            var toDeleteFiles = new List<string>();

            await _uow.BeginTransactionAsync(ct);
            try
            {
                foreach (var img in toRemove)
                {
                    toDeleteFiles.Add(img.FileName);
                    r.Images.Remove(img);
                }

                // ضمان وجود صورة أساسية واحدة
                if (r.Images.Count > 0 && !r.Images.Any(i => i.IsPrimary))
                    r.Images.First().IsPrimary = true;

                r.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveAndCommitAsync(ct);
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }

            // بعد الـ Commit: حذف الملفات فعلياً من التخزين
            foreach (var filename in toDeleteFiles)
            {
                try { await _fileService.DeleteAsync(filename, ct); } catch { /* ولا يهمك */ }
            }

            // رجّع الطلب مع الصور/الملاحظات إلخ..
            var withIncludes = await _repository.Query(
                    asTracking: false,
                    include: q => q
                        .Include(x => x.Images)
                        .Include(x => x.Notes)
                        .Include(x => x.ProblemType).ThenInclude(pt => pt.Translations),
                    predicate: x => x.Id == requestId)
                .FirstOrDefaultAsync(ct);


            var fresh = await GetByIdAsync(requestId, userId, userRole, language, ct);
            return (fresh, "Images_Removed");
        }


        private static NoteAuthor InferAuthor(bool isOwner, bool isTech, bool isMgr, bool isAdmin)
        {
            if (isAdmin) return NoteAuthor.Admin;
            if (isMgr) return NoteAuthor.Manager;
            if (isTech) return NoteAuthor.Technician;
            return NoteAuthor.Owner;
        }

        private async Task SendStatusChangeNotificationAsync(
            MaintenanceRequest r,
            CaseType newCase,
            string language,
            CancellationToken ct)
        {
            // 1) جهّز قائمة المستلمين بدون تكرار
            var recipients = new HashSet<string>(StringComparer.Ordinal);

            string titleKey;
            string bodyKey;
            object[]? bodyArgs;

            //  صاحب الطلب: نستثنيه فقط لو الحالة ManagerReview
            if (newCase != CaseType.ManagerReview &&
                !string.IsNullOrWhiteSpace(r.OwnerUserId))
            {
                recipients.Add(r.OwnerUserId);
            }

            // الفنيين المعيَّنين نشطًا على الطلب
            var techIds = await _reqTechRepo.GetActiveTechniciansAsync(r.Id, ct);
            if (techIds is { Count: > 0 })
            {
                foreach (var tid in techIds)
                {
                    if (!string.IsNullOrWhiteSpace(tid))
                        recipients.Add(tid);
                }
            }

            // المدراء (Role = MaintenanceManager) + Admin
            await AddRoleRecipientsAsync(recipients, "MaintenanceManager", null, ct);
            await AddRoleRecipientsAsync(recipients, "Admin", null, ct);

            // لو ما في ولا مستلم، خلص
            if (recipients.Count == 0)
                return;

            // 2) نوع الإشعار، الشدة، العنوان، النص
            var type = newCase == CaseType.Completed
                ? NotificationType.RequestCompleted
                : NotificationType.RequestStatusChanged;

            var severity = newCase == CaseType.Completed
                ? NotificationSeverity.Success
                : NotificationSeverity.Info;

            var title = newCase == CaseType.Completed
                ? "تم إكمال طلب الصيانة"
                : "تم تغيير حالة طلب الصيانة";

            var body = newCase == CaseType.Completed
                ? $"تم إكمال طلب الصيانة رقم {r.Id}."
                : $"تم تغيير حالة طلب الصيانة رقم {r.Id} إلى {newCase}.";

            if (newCase == CaseType.Completed)
            {
                titleKey = "NOTIF_REQUEST_COMPLETED_TITLE";
                bodyKey = "NOTIF_REQUEST_COMPLETED_BODY";
                bodyArgs = new object[] { r.Id };
            }
            else
            {
                titleKey = "NOTIF_REQUEST_STATUS_CHANGED_TITLE";
                bodyKey = "NOTIF_REQUEST_STATUS_CHANGED_BODY";
                bodyArgs = new object[] { r.Id, newCase.ToString() };
            }

            // 3) إرسال نفس الإشعار لكل مستلم
            foreach (var userId in recipients)
            {
                await _notificationService.CreateAsync(new NotificationCreateModel
                {
                    UserId = userId,
                    MaintenanceRequestId = r.Id,
                    Type = type,
                    Severity = severity,
                    Language = language,
                    // ✅ localization
                    TitleKey = titleKey,
                    BodyKey = bodyKey,
                    BodyArgs = bodyArgs,

                    Channels = NotificationChannel.InApp | NotificationChannel.Email
                }, ct);
            }
        }

        private async Task SendNoteAddedNotificationAsync(
            MaintenanceRequest r,
            NoteType noteType,
            string authorUserId,
            string noteText,
            string language,
            CancellationToken ct)
        {
            // جهّز قائمة المستلمين (بدون تكرار)
            var recipients = new HashSet<string>(StringComparer.Ordinal);

            // صاحب الطلب (إن لم يكن هو الكاتب)
            if (!string.IsNullOrWhiteSpace(r.OwnerUserId) &&
                !string.Equals(r.OwnerUserId, authorUserId, StringComparison.Ordinal))
            {
                recipients.Add(r.OwnerUserId);
            }

            // الفنيّون المعيّنون نشطًا (غير الكاتب)
            var techIds = await _reqTechRepo.GetActiveTechniciansAsync(r.Id, ct);
            if (techIds is { Count: > 0 })
            {
                foreach (var tid in techIds)
                {
                    if (!string.IsNullOrWhiteSpace(tid) &&
                        !string.Equals(tid, authorUserId, StringComparison.Ordinal))
                    {
                        recipients.Add(tid);
                    }
                }
            }

            // المدراء + الأدمن (غير الكاتب)
            await AddRoleRecipientsAsync(recipients, "MaintenanceManager", authorUserId, ct);
            await AddRoleRecipientsAsync(recipients, "Admin", authorUserId, ct);

            if (recipients.Count == 0)
                return;

            var title = "تم إضافة ملاحظة جديدة على طلب الصيانة";

            var trimmed = (noteText ?? string.Empty).Trim();
            if (trimmed.Length > 120)
                trimmed = trimmed.Substring(0, 120) + "...";

            var body = $"تمت إضافة ملاحظة جديدة على طلب الصيانة رقم {r.Id}: {trimmed}";

            foreach (var uid in recipients)
            {
                await _notificationService.CreateAsync(new NotificationCreateModel
                {
                    UserId = uid,
                    MaintenanceRequestId = r.Id,
                    Type = NotificationType.RequestStatusChanged,
                    Severity = NotificationSeverity.Info,
                    Language = language,
                    // ✅ localization
                    TitleKey = "NOTIF_NOTE_ADDED_TITLE",
                    BodyKey = "NOTIF_NOTE_ADDED_BODY",
                    BodyArgs = new object[] { r.Id, trimmed },

                    Channels = NotificationChannel.InApp | NotificationChannel.Email
                }, ct);
            }
        }

        private async Task AddRoleRecipientsAsync(
            HashSet<string> recipients,
            string roleName,
            string? excludeUserId,
            CancellationToken ct)
        {
            var users = await _userRepo.GetByRoleAsync(roleName, ct);
            if (users is not { Count: > 0 })
                return;

            foreach (var u in users)
            {
                if (string.IsNullOrWhiteSpace(u.Id))
                    continue;

                if (!string.IsNullOrWhiteSpace(excludeUserId) &&
                    string.Equals(u.Id, excludeUserId, StringComparison.Ordinal))
                    continue;

                recipients.Add(u.Id);
            }
        }

        private async Task SendRequestUpdatedByOwnerNotificationAsync(
            MaintenanceRequest r,
            string language,
            CancellationToken ct)
        {
            // قائمة المستلمين بدون تكرار
            var recipients = new HashSet<string>(StringComparer.Ordinal);

            // الفنيين المعيّنين نشطًا على الطلب
            var techIds = await _reqTechRepo.GetActiveTechniciansAsync(r.Id, ct);
            if (techIds is { Count: > 0 })
            {
                foreach (var tid in techIds)
                {
                    if (!string.IsNullOrWhiteSpace(tid))
                        recipients.Add(tid);
                }
            }

            // المدراء + الأدمن
            await AddRoleRecipientsAsync(recipients, "MaintenanceManager", null, ct);
            await AddRoleRecipientsAsync(recipients, "Admin", null, ct);

            // تأكد ما يوصل للمالك حتى لو كان ضمن أي مجموعة
            if (!string.IsNullOrWhiteSpace(r.OwnerUserId))
                recipients.Remove(r.OwnerUserId);

            if (recipients.Count == 0)
                return;

            var title = "تم تعديل طلب الصيانة";
            var body = $"قام صاحب الطلب بتعديل طلب الصيانة رقم {r.Id}.";

            foreach (var uid in recipients)
            {
                await _notificationService.CreateAsync(new NotificationCreateModel
                {
                    UserId = uid,
                    MaintenanceRequestId = r.Id,
                    Type = NotificationType.RequestStatusChanged,
                    Severity = NotificationSeverity.Info,
                    Language = language,
                    // ✅ localization
                    TitleKey = "NOTIF_REQUEST_UPDATED_TITLE",
                    BodyKey = "NOTIF_REQUEST_UPDATED_BODY",
                    BodyArgs = new object[] { r.Id },

                    Channels = NotificationChannel.InApp | NotificationChannel.Email
                }, ct);
            }
        }


        private async Task EnrichAssignedTechniciansNamesAsync(
            MaintenanceRequestResponseDTO dto,
            string language,
            CancellationToken ct = default)
        {
            if (dto?.AssignedTechnicians == null || dto.AssignedTechnicians.Count == 0)
                return;

            var ids = dto.AssignedTechnicians
                .Select(x => x.UserId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (ids.Count == 0)
                return;

            var dict = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var id in ids)
            {
                var u = await _userRepo.GetByIdAsync(id, ct);
                if (u is null) continue;

                // 👈 هون التعديل: استخدم الاسم حسب اللغة
                dict[id] = u.GetDisplayName(language);
            }

            foreach (var t in dto.AssignedTechnicians)
            {
                if (!string.IsNullOrWhiteSpace(t.UserId) &&
                    dict.TryGetValue(t.UserId, out var name))
                {
                    t.FullName = name;
                }
            }
        }

        private static bool IsValidCaseType(CaseType value)
    => Enum.IsDefined(typeof(CaseType), (int)value);

        private static bool IsValidPriority(Priority value)
    => Enum.IsDefined(typeof(Priority), (int)value);

        private static bool IsValidNoteType(NoteType value)
            => Enum.IsDefined(typeof(NoteType), (int)value);

        private static bool IsValidTransition(CaseType current, CaseType target)
        {

            if (current == target)
                return true;

            // الحالات اللي بتتغيّر داخلياً فقط – ما منسمح لحد يغير إلها يدويًا
            if (target is CaseType.Submitted or CaseType.Modified)
                return false;

            // خريطة للحركات المسموحة من كل حالة
            return current switch
            {
                // الطلب بعد الإنشاء / التعديل
                CaseType.Submitted => target is CaseType.Processing or CaseType.Cancelled,
                CaseType.Modified => target is CaseType.Processing or CaseType.Cancelled,

                // قيد المعالجة
                CaseType.Processing => target is
                    CaseType.ManagerReview      // يروح لمراجعة مدير
                    or CaseType.ResourcesNeeded // الفني محتاج مساعدة/موارد
                    or CaseType.Completed
                    or CaseType.Cancelled,

                // تحت مراجعة مدير
                CaseType.ManagerReview => target is
                    CaseType.Processed
                    or CaseType.ResourcesNeeded
                    or CaseType.Completed
                    or CaseType.Cancelled,

                // يحتاج موارد
                CaseType.ResourcesNeeded => target is
                    CaseType.ManagerReview
                    or CaseType.Completed
                    or CaseType.Cancelled,

                // تمت المعالجة
                CaseType.Processed => target is
                CaseType.Reopened
                or CaseType.Completed
                    or CaseType.Cancelled,

                // الطلب مغلق
                CaseType.Completed => target is
                    CaseType.Reopened,   // مسموح فقط إعادة فتحه

                CaseType.Cancelled => target is
                    CaseType.Reopened,   // نفس الشي

                // بعد إعادة الفتح، ما بنرجع لبداية الفlow، بنكمّل لقدّام
                CaseType.Reopened => target is
                    CaseType.Processing
                    or CaseType.ManagerReview
                    or CaseType.ResourcesNeeded
                    or CaseType.Completed
                    or CaseType.Modified
                    or CaseType.Cancelled,

                _ => false
            };
        }


    }



}





