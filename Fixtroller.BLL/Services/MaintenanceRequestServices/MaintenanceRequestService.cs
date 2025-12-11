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

        public MaintenanceRequestService(
            IMaintenanceRequestRepository repository,
            ITechnicianRepository techRepo,
            IFileService fileService,
            IUnitOfWork uow,
            IWorkTimeRepository workRepo,
            IMaintenanceRequestTechnicianRepository reqTechRepo,
            INotificationService notificationService,
            IUserRepository userRepo
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
        }

        public async Task<int> CreateWithFile(MaintenanceRequestRequestDTO request, string userId, CancellationToken ct = default)
        {
            // جهّز الكيان
            var entity = MaintenanceRequestMapper.ToEntity(
                     request,
                     ownerUserId: userId,
                     createdByUserId: userId);

            // 1) ارفع الملفات "خارج" الترانزاكشن + لائحة للتعويض عند الفشل
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

            await _uow.BeginTransactionAsync(ct);
            try
            {
                await _repository.AddAsync(entity);
                await _uow.SaveAndCommitAsync(ct);

                var managers = await _userRepo.GetByRoleAsync("MaintenanceManager", ct);
                if (managers is { Count: > 0 })
                {
                    foreach (var mgr in managers)
                    {
                        if (string.IsNullOrWhiteSpace(mgr.Id))
                            continue;

                        await _notificationService.CreateAsync(new NotificationCreateModel
                        {
                            UserId = mgr.Id,
                            MaintenanceRequestId = entity.Id,
                            Type = NotificationType.RequestStatusChanged,   // تقدر تعمل نوع خاص مثلاً RequestCreated لو ضفته في enum
                            Severity = NotificationSeverity.Info,
                            Title = "تم إنشاء طلب صيانة جديد",
                            Body = $"تم إنشاء طلب صيانة جديد برقم {entity.Id}.",
                            Channels = NotificationChannel.InApp | NotificationChannel.Email
                        }, ct);
                    }
                }


                return entity.Id;
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                foreach (var name in uploaded)
                {
                    try { await _fileService.DeleteAsync(name, ct); } catch { }
                }
                throw;
            }
        }


        public async Task<(int? Id, string MessageKey)> CreateScenarioAsync(
            MaintenanceRequestScenarioRequestDTO request,
            string callerUserId,
            string callerRole,
            CancellationToken ct = default)
        {
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

                // 🔔 إشعار لصاحب الطلب بأن تم إنشاء طلب له
                if (!string.IsNullOrWhiteSpace(entity.OwnerUserId))
                {
                    await _notificationService.CreateAsync(new NotificationCreateModel
                    {
                        UserId = entity.OwnerUserId,
                        MaintenanceRequestId = entity.Id,
                        Type = NotificationType.RequestStatusChanged,
                        Severity = NotificationSeverity.Info,
                        Title = "تم إنشاء طلب صيانة لك",
                        Body = $"تم إنشاء طلب صيانة جديد برقم {entity.Id}.",
                        Channels = NotificationChannel.InApp | NotificationChannel.Email
                    }, ct);
                }

                // 🔔 إشعارات لمدراء الصيانة عن طلب جديد
                var managers = await _userRepo.GetByRoleAsync("MaintenanceManager", ct);
                if (managers is { Count: > 0 })
                {
                    foreach (var mgr in managers)
                    {
                        if (string.IsNullOrWhiteSpace(mgr.Id))
                            continue;

                        await _notificationService.CreateAsync(new NotificationCreateModel
                        {
                            UserId = mgr.Id,
                            MaintenanceRequestId = entity.Id,
                            Type = NotificationType.RequestStatusChanged,   // تقدر تضيف نوع جديد لاحقاً
                            Severity = NotificationSeverity.Info,
                            Title = "تم إنشاء طلب صيانة جديد (سيناريو)",
                            Body = $"تم إنشاء طلب صيانة جديد برقم {entity.Id} عن طريق {callerRole}.",
                            Channels = NotificationChannel.InApp | NotificationChannel.Email
                        }, ct);
                    }
                }

                // نجاح: نرجّع Id + MessageKey جاهز للترجمة في الـ PL
                return (entity.Id, "Created"); // "Created" موجودة في SharedResource
            }
            catch
            {
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
     int pageNumber = 1,
     int pageSize = 10,
     CancellationToken ct = default)
        {
            language = string.IsNullOrWhiteSpace(language) ? "ar" : language;

            // تأمين القيم
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            // 1) أساس الكويري + Include للـ ProblemType وترجماته
            var q = _repository.Query(
                    asTracking: false,
                    predicate: x =>
                        x.Status == Status.Active &&
                        x.OwnerUserId == userId)
                .Include(x => x.ProblemType)
                    .ThenInclude(pt => pt.Translations);

            // 2) إجمالي عدد السجلات
            var totalCount = await q.CountAsync(ct);

            // 3) عدد الصفحات (لو مافي سجلات = 0 صفحات)
            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            // 4) جلب الصفحة المطلوبة + نحسب اسم نوع المشكلة
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

                    IsOwner = true   // لأنه mine
                })
                .ToListAsync(ct);

            // 5) المابر إلى DTO
            var data = pagedRows
                .Select(r => MaintenanceRequestMapper.ToMineListItem(
                    r.Light,
                    role,
                    r.IsOwner,
                    language,
                    r.ProblemTypeName))
                .ToList();

            // 6) النتيجة النهائية
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

            // 1) الكويري الأساسي مع Include للـ ProblemType + Translations
            var q = _repository.Query(
                    asTracking: false,
                    predicate: x => x.Status == Status.Active)
                .Include(x => x.ProblemType)
                    .ThenInclude(pt => pt.Translations);

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
                        Description = x.Description,
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
                if (user is not null && !string.IsNullOrWhiteSpace(user.FullName))
                {
                    techNames[techId!] = user.FullName;
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
                            .Include(r => r.Technicians.Where(t => t.UnassignedAtUtc == null))
                             .Include(r => r.ProblemType)
                                .ThenInclude(pt => pt.Translations)
                    )
                    .FirstOrDefaultAsync(ct);

            if (e is null) return null;

            var isOwner = string.Equals(e.OwnerUserId, userId, StringComparison.Ordinal);


            var dto = MaintenanceRequestMapper.ToResponse(e, role, _fileService.GetPublicUrl, language, isOwner, includeOwnerDetails: isAdmin || isManager);

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

            return dto;
        }

        public async Task<(AssignTechnicianResponseDTO? Response, string MessageKey)> AssignTechniciansAsync(
     int requestId,
     IEnumerable<string> technicianUserIds,
     int? expectedDuration,
     string language = "ar",
     CancellationToken ct = default)
        {
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

                // 👈 ما في removed، بالتالي ما بنوقف أي مؤقتات هنا

                // لو Submitted ارفعها إلى Processing
                if (request.CaseType == CaseType.Submitted)
                    request.CaseType = CaseType.Processing;

                request.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveAndCommitAsync(ct);

                // إشعارات للفنيين الجدد فقط
                foreach (var tid in added)
                {
                    await _notificationService.CreateAsync(new NotificationCreateModel
                    {
                        UserId = tid,
                        MaintenanceRequestId = request.Id,
                        Type = NotificationType.RequestAssigned,
                        Severity = NotificationSeverity.Info,
                        Title = "تم تعيينك لطلب صيانة جديد",
                        Body = $"تم تعيينك لطلب الصيانة رقم {request.Id}.",
                        Channels = NotificationChannel.InApp | NotificationChannel.Email
                    }, ct);
                }

                var loaded = await _repository.GetForAssignmentAsync(requestId, ct);
                var response = TechnicianMappings.ToAssignTechnicianResponse(loaded!, language);
                await EnrichAssignTechnicianResponseAsync(response, ct);
                return (response, "Technicians_Assigned");
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        private async Task EnrichAssignTechnicianResponseAsync(
    AssignTechnicianResponseDTO? res,
    CancellationToken ct)
        {
            if (res == null) return;

            // اجمع كل IDs (الفني الرئيسي + كل النشطين)
            var ids = res.ActiveTechnicians
                .Select(t => t.Id)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (res.Technician != null &&
                !string.IsNullOrWhiteSpace(res.Technician.Id) &&
                !ids.Contains(res.Technician.Id, StringComparer.Ordinal))
            {
                ids.Add(res.Technician.Id);
            }

            if (ids.Count == 0) return;

            // حمل بيانات المستخدمين من الـ UserRepository
            var dict = new Dictionary<string, (string fullName, string email)>(StringComparer.Ordinal);

            foreach (var id in ids)
            {
                var user = await _userRepo.GetByIdAsync(id, ct);
                if (user is null) continue;

                dict[id] = (user.FullName, user.Email);
            }

            // فانكشن صغيرة لتطبيق البيانات على dto موجود
            void Apply(TechnicianResponseDTO t)
            {
                if (t == null) return;
                if (dict.TryGetValue(t.Id, out var info))
                {
                    t.FullName = info.fullName;
                    t.Email = info.email;
                }
            }

            foreach (var t in res.ActiveTechnicians)
                Apply(t);

            if (res.Technician != null)
                Apply(res.Technician);
        }

        public async Task<(AssignTechnicianResponseDTO? Response, string MessageKey)> AssignTechnicianAsync(
            int requestId, string technicianUserId, int? expectedDuration, string language = "ar", CancellationToken ct = default)
        {
            var request = await _repository.GetForAssignmentAsync(requestId, ct);
            if (request is null) return (null, "Request_NotFound");

            var tech = await _techRepo.GetByIdAsync(technicianUserId, ct);
            if (tech is null) return (null, "Technician_NotFound");

            var isTechnician = await _techRepo.IsInRoleAsync(technicianUserId, "Technician", ct);
            if (!isTechnician) return (null, "User_NotTechnician");

            // إن كان مُعيَّن نشطًا أصلًا، لا تغيّر شيء
            var already = await _reqTechRepo.IsActiveAssignedAsync(requestId, technicianUserId, ct);
            if (already)
            {
                var loadedNoop = await _repository.GetForAssignmentAsync(requestId, ct);
                var response = TechnicianMappings.ToAssignTechnicianResponse(loadedNoop!, language);
                await EnrichAssignTechnicianResponseAsync(response, ct);
                return (response, "Technician_AlreadyAssigned");
            }

            await _uow.BeginTransactionAsync(ct);
            try
            {
                await _reqTechRepo.AddActiveAsync(requestId, technicianUserId, expectedDuration, ct);

                if (request.CaseType == CaseType.Submitted)
                    request.CaseType = CaseType.Processing;

                request.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveAndCommitAsync(ct);


                await _notificationService.CreateAsync(new NotificationCreateModel
                {
                    UserId = technicianUserId,
                    MaintenanceRequestId = request.Id,
                    Type = NotificationType.RequestAssigned,
                    Severity = NotificationSeverity.Info,
                    Title = "تم تعيينك لطلب صيانة جديد",
                    Body = $"تم تعيينك لطلب الصيانة رقم {request.Id}.",
                    Channels = NotificationChannel.InApp | NotificationChannel.Email
                }, ct);


                var loaded = await _repository.GetForAssignmentAsync(requestId, ct);
                var response = TechnicianMappings.ToAssignTechnicianResponse(loaded!, language);
                await EnrichAssignTechnicianResponseAsync(response, ct);
                return (response, "Technician_Assigned");
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<(bool ok, string messageKey)> RemoveTechnicianAsync(
      int requestId,
      string technicianUserId,
      CancellationToken ct = default)
        {
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

                // 🔔 إشعار للفني الذي تمّت إزالته
                await _notificationService.CreateAsync(new NotificationCreateModel
                {
                    UserId = technicianUserId,
                    MaintenanceRequestId = r.Id,
                    Type = NotificationType.RequestStatusChanged,
                    Severity = NotificationSeverity.Info,
                    Title = "تمت إزالتك من طلب صيانة",
                    Body = $"تمت إزالتك من طلب الصيانة رقم {r.Id}.",
                    Channels = NotificationChannel.InApp | NotificationChannel.Email
                }, ct);

                // 🔔 إشعارات لمدراء الصيانة (اختياري بس منطقي)
                var managers = await _userRepo.GetByRoleAsync("MaintenanceManager", ct);
                if (managers is { Count: > 0 })
                {
                    foreach (var mgr in managers)
                    {
                        if (string.IsNullOrWhiteSpace(mgr.Id))
                            continue;

                        await _notificationService.CreateAsync(new NotificationCreateModel
                        {
                            UserId = mgr.Id,
                            MaintenanceRequestId = r.Id,
                            Type = NotificationType.RequestStatusChanged,
                            Severity = NotificationSeverity.Info,
                            Title = "تم إزالة فني من طلب صيانة",
                            Body = $"تمت إزالة فني من طلب الصيانة رقم {r.Id}.",
                            Channels = NotificationChannel.InApp | NotificationChannel.Email
                        }, ct);
                    }
                }

                return (true, "Technician_Removed");
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<(bool ok, string messageKey)> StartWorkAsync(
           int requestId,
           string technicianUserId,
           string callerUserId,
           string callerRole,
           CancellationToken ct = default)
        {
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

                // ( إشعار للفني )
                await _notificationService.CreateAsync(new NotificationCreateModel
                {
                    UserId = technicianUserId,
                    MaintenanceRequestId = req.Id,
                    Type = NotificationType.RequestStatusChanged, // ممكن تعمل نوع خاص لاحقًا
                    Severity = NotificationSeverity.Info,
                    Title = "تم بدء العمل على طلب صيانة",
                    Body = $"تم بدء العمل على طلب الصيانة رقم {req.Id}.",
                    Channels = NotificationChannel.InApp | NotificationChannel.Email
                }, ct);

                return (true, "Work_Started");
            }
            catch
            {
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
            // تحقّقات بدون ترانزاكشن
            var r = await _repository.GetForUpdateAsync(requestId, ct);
            if (r is null) return (null, "Request_NotFound");

            var newCase = dto.NewCaseType;

            bool isManager = userRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            bool isAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            bool isTechnician = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            bool isOwner = r.OwnerUserId == userId;

            var useOwnerPath = isOwner && (preferOwnerPath || !(isManager || isAdmin || isTechnician));
            var author = InferAuthor(isOwner, isTechnician, isManager, isAdmin);

            if (r.CaseType == newCase)
            {
                var respNoChange = MaintenanceRequestMapper.ToResponse(r, userRole, _fileService.GetPublicUrl, language, isOwner);
                return (respNoChange, "Case_NoChange");
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

                    
                    await SendStatusChangeNotificationAsync(r, newCase, ct);

                    var resp = MaintenanceRequestMapper.ToResponse(r, userRole, _fileService.GetPublicUrl, language, isOwner);
                    return (resp, "Case_Changed");
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

                    await SendStatusChangeNotificationAsync(r, newCase, ct);

                    var respTech = MaintenanceRequestMapper.ToResponse(r, userRole, _fileService.GetPublicUrl, language, isOwner);
                    return (respTech, "Case_Changed");
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

                
                await SendStatusChangeNotificationAsync(r, newCase, ct);

                var respMgr = MaintenanceRequestMapper.ToResponse(r, userRole, _fileService.GetPublicUrl, language, isOwner);
                return (respMgr, "Case_Changed");
            }
            catch
            {
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

            var noteType = dto.Type ?? NoteType.General;

            await _uow.BeginTransactionAsync(ct);
            try
            {
                r.Notes.Add(MaintenanceRequestMapper.ToNote(dto.Text!, noteType, author, userId, r.Id));
                r.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveAndCommitAsync(ct);

                // 🔔 إشعار بإضافة ملاحظة جديدة على الطلب
                await SendNoteAddedNotificationAsync(r, noteType, userId, dto.Text!, ct);

                return (MaintenanceRequestMapper.ToResponse(r, userRole, _fileService.GetPublicUrl, language, isOwner),
                        "Note_Added");
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

                // 9) إشعارات للمديرين والفنيين أن صاحب الطلب عدّل الطلب 👇
                await SendRequestUpdatedByOwnerNotificationAsync(r, ct);

                var isOwner = true; // مؤكّد من الفحص أعلاه
                var response = MaintenanceRequestMapper.ToResponse(r, role, _fileService.GetPublicUrl, language, isOwner);
                return (response, "Request_Updated");
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

            var dtoRes = withIncludes is null
                ? null
                : MaintenanceRequestMapper.ToResponse(withIncludes, userRole, _fileService.GetPublicUrl, language,
                      isOwner: string.Equals(withIncludes.CreatedByUserId, userId, StringComparison.Ordinal));

            return (dtoRes, "Images_Added");
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

            var dtoRes = withIncludes is null
                ? null
                : MaintenanceRequestMapper.ToResponse(
                      withIncludes,
                      userRole,
                      _fileService.GetPublicUrl,
                      language,
                      isOwner: string.Equals(withIncludes.CreatedByUserId, userId, StringComparison.Ordinal));

            // 🔑 مسج جديدة للترجمة
            return (dtoRes, "Images_Removed");
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
            CancellationToken ct)
        {
            // 1) جهّز قائمة المستلمين بدون تكرار
            var recipients = new HashSet<string>(StringComparer.Ordinal);

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

            // المدراء (Role = MaintenanceManager)
            var managers = await _userRepo.GetByRoleAsync("MaintenanceManager", ct);
            if (managers is { Count: > 0 })
            {
                foreach (var mgr in managers)
                {
                    if (!string.IsNullOrWhiteSpace(mgr.Id))
                        recipients.Add(mgr.Id);
                }
            }

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

            // 3) إرسال نفس الإشعار لكل مستلم
            foreach (var userId in recipients)
            {
                await _notificationService.CreateAsync(new NotificationCreateModel
                {
                    UserId = userId,
                    MaintenanceRequestId = r.Id,
                    Type = type,
                    Severity = severity,
                    Title = title,
                    Body = body,
                    Channels = NotificationChannel.InApp | NotificationChannel.Email
                }, ct);
            }
        }

        private async Task SendNoteAddedNotificationAsync(
    MaintenanceRequest r,
    NoteType noteType,
    string authorUserId,
    string noteText,
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

            // المدراء (غير الكاتب)
            var managers = await _userRepo.GetByRoleAsync("MaintenanceManager", ct);
            if (managers is { Count: > 0 })
            {
                foreach (var mgr in managers)
                {
                    if (!string.IsNullOrWhiteSpace(mgr.Id) &&
                        !string.Equals(mgr.Id, authorUserId, StringComparison.Ordinal))
                    {
                        recipients.Add(mgr.Id);
                    }
                }
            }

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
                    Type = NotificationType.RequestStatusChanged, // لو عندك نوع خاص NoteAdded حاب تضيفه لاحقاً، استعمله هنا
                    Severity = NotificationSeverity.Info,
                    Title = title,
                    Body = body,
                    Channels = NotificationChannel.InApp | NotificationChannel.Email
                }, ct);
            }
        }


        private async Task SendRequestUpdatedByOwnerNotificationAsync(
    MaintenanceRequest r,
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

            // المدراء (MaintenanceManager)
            var managers = await _userRepo.GetByRoleAsync("MaintenanceManager", ct);
            if (managers is { Count: > 0 })
            {
                foreach (var mgr in managers)
                {
                    if (!string.IsNullOrWhiteSpace(mgr.Id))
                        recipients.Add(mgr.Id);
                }
            }

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
                    Type = NotificationType.RequestStatusChanged, // لو حاب لاحقاً تعمل نوع خاص RequestUpdatedByOwner تمام
                    Severity = NotificationSeverity.Info,
                    Title = title,
                    Body = body,
                    Channels = NotificationChannel.InApp | NotificationChannel.Email
                }, ct);
            }
        }


    }



}





