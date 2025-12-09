using Fixtroller.BLL.Mapping;
using Fixtroller.BLL.Reports;
using Fixtroller.BLL.Services.FileService;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.Reports;
using Fixtroller.DAL.Repositories.MaintenanceRequestRepositories;
using Fixtroller.DAL.Repositories.UserRepository;
using Fixtroller.DAL.Repositories.UserRepository.TechnicianRepositorirs;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Fixtroller.BLL.Services.ReportsServices
{
    public sealed class MaintenanceReportsService : IMaintenanceReportsService
    {
        private readonly IMaintenanceRequestRepository _requestRepository;
        private readonly IWorkTimeRepository _workTimeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITechnicianRepository _technicianRepository;
        private readonly IFileService _fileService;

        public MaintenanceReportsService(
            IMaintenanceRequestRepository requestRepository,
            IWorkTimeRepository workTimeRepository,
            IUserRepository userRepository,
            ITechnicianRepository technicianRepository,
            IFileService fileService)
        {
            _requestRepository = requestRepository;
            _workTimeRepository = workTimeRepository;
            _userRepository = userRepository;
            _technicianRepository = technicianRepository;
            _fileService = fileService;
        }

        public async Task<(SingleRequestReportDTO? Report, string MessageKey)> GetSingleRequestAsync(
            int requestId,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var isAdmin = string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase);
            var isManager = string.Equals(userRole, "MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isEmployee = string.Equals(userRole, "Employee", StringComparison.OrdinalIgnoreCase);
            var isTechnician = string.Equals(userRole, "Technician", StringComparison.OrdinalIgnoreCase);

            // تحميل الطلب مع العلاقات المهمة
            var query = _requestRepository.Query(
                asTracking: false,
                include: q => q
                    .Include(r => r.ProblemType)
                        .ThenInclude(pt => pt.Translations)
                    .Include(r => r.OwnerUser)
                    .Include(r => r.CreatedByUser)
                    .Include(r => r.Technicians)
                    .Include(r => r.Notes)
                    .ThenInclude(n => n.CreatedByUser),
                predicate: x =>
                    x.Id == requestId &&
                    (
                        isAdmin ||
                        isManager ||
                        (isEmployee && x.OwnerUserId == userId) ||
                        (isTechnician && (x.CreatedByUserId == userId ||
                                          x.Technicians.Any(t => t.TechnicianUserId == userId && t.UnassignedAtUtc == null)))
                    ));

            var entity = await query.FirstOrDefaultAsync(ct);
            if (entity is null)
                return (null, "Request_NotFound");

            var isOwner = string.Equals(entity.OwnerUserId, userId, StringComparison.Ordinal);

            // نعيد استخدام المابر الحالي للحصول على أسماء الـ Priority/Case/ProblemType مترجمة
            var baseDto = MaintenanceRequestMapper.ToResponse(
                entity,
                userRole,
                _fileService.GetPublicUrl,
                language,
                isOwner);

            // الأوقات من WorkTimeEntry
            var workEntries = await _workTimeRepository.Query(asTracking: false)
                .Where(w => w.RequestId == requestId)
                .ToListAsync(ct);

            // First Assigned
            var firstAssigned = entity.Technicians
                .OrderBy(t => t.AssignedAtUtc)
                .FirstOrDefault();

            // First work start
            DateTime? firstWorkStart = null;
            if (workEntries.Count > 0)
            {
                firstWorkStart = workEntries.Min(w => (DateTime?)w.StartedAt.UtcDateTime);
            }

            // SLA على مستوى الطلب: نأخذ أول ExpectedDuration موجود (بساعات)
            int? slaHours = entity.Technicians
                .Where(t => t.ExpectedDuration.HasValue)
                .OrderBy(t => t.AssignedAtUtc)
                .Select(t => t.ExpectedDuration)
                .FirstOrDefault();

            // مدة الإغلاق الفعلية
            double? actualHours = null;
            if (entity.ClosedAtUtc is not null)
            {
                actualHours = (entity.ClosedAtUtc.Value - entity.CreatedAt).TotalHours;
            }
            else
            {
                // طلب مفتوح: نقدر نحسب المدة الحالية لو حبّينا
                actualHours = (DateTime.UtcNow - entity.CreatedAt).TotalHours;
            }

            bool? isWithinSla = null;
            if (slaHours.HasValue && actualHours.HasValue && entity.ClosedAtUtc is not null)
            {
                isWithinSla = actualHours.Value <= slaHours.Value;
            }

            // الفنيين
            var techIds = entity.Technicians
                .Select(t => t.TechnicianUserId)
                .Distinct()
                .ToList();

            var technicians = new List<SingleRequestReportTechnicianDTO>();

            foreach (var techLink in entity.Technicians.OrderBy(t => t.AssignedAtUtc))
            {
                // 👈 استخدم الريبو الخاص بالفنيين
                var techUser = await _technicianRepository.GetByIdAsync(techLink.TechnicianUserId, ct);

                var techName = techUser?.FullName ?? techLink.TechnicianUserId;

                string? techCategory = null;

                var trans = techUser?.TechnicianCategory?.Translations;

                if (trans != null && trans.Count > 0)
                {
                    var best = trans
                        .OrderBy(tr =>
                            tr.Language.Equals(language, StringComparison.OrdinalIgnoreCase) ? 0 :
                            tr.Language.Equals("ar", StringComparison.OrdinalIgnoreCase) ? 1 : 2)
                        .FirstOrDefault();

                    techCategory = best?.Name;
                }

                var techEntries = workEntries
                    .Where(w => w.TechnicianUserId == techLink.TechnicianUserId)
                    .ToList();

                DateTime? tFirstStart = null;
                DateTime? tLastStop = null;
                double totalMinutes = 0;

                if (techEntries.Count > 0)
                {
                    tFirstStart = techEntries.Min(w => w.StartedAt.UtcDateTime);
                    tLastStop = techEntries
                        .Where(w => w.StoppedAt != null)
                        .Select(w => (DateTime?)w.StoppedAt!.Value.UtcDateTime)
                        .Max();

                    totalMinutes = techEntries
                        .Where(w => w.StoppedAt != null)
                        .Sum(w => (w.StoppedAt!.Value - w.StartedAt).TotalMinutes);
                }

                technicians.Add(new SingleRequestReportTechnicianDTO
                {
                    TechnicianUserId = techLink.TechnicianUserId,
                    TechnicianName = techName,
                    TechnicianCategory = techCategory,      // 👈 هان رح تنعبي فعليًا
                    AssignedAtUtc = techLink.AssignedAtUtc,
                    UnassignedAtUtc = techLink.UnassignedAtUtc,
                    FirstWorkStartedAtUtc = tFirstStart,
                    LastWorkStoppedAtUtc = tLastStop,
                    TotalWorkMinutes = totalMinutes,
                    ExpectedDurationHours = techLink.ExpectedDuration
                });
            }

            var report = new SingleRequestReportDTO
            {
                RequestId = baseDto.Id,
                Title = baseDto.Title,
                Description = baseDto.Description,
                ProblemTypeName = baseDto.ProblemTypeName ?? "",
                PriorityName = baseDto.PriorityName,
                CaseTypeName = baseDto.CaseType,

                OwnerFullName = entity.OwnerUser?.FullName ?? entity.OwnerUserId,
                OwnerDepartment = entity.OwnerUser?.Department,
                OwnerLocation = entity.OwnerUser?.Location,
                RequestAddress = baseDto.Address,

                IsCreatedByOwner = baseDto.IsCreatedByOwner,
                CreatedByFullName = entity.CreatedByUser?.FullName ?? entity.CreatedByUserId,

                CreatedAtUtc = baseDto.CreatedAt,
                FirstAssignedAtUtc = firstAssigned?.AssignedAtUtc,
                FirstWorkStartedAtUtc = firstWorkStart,
                ClosedAtUtc = baseDto.ClosedAtUtc,

                ExpectedDurationHours = slaHours,
                ActualDurationHours = actualHours,
                IsWithinSla = isWithinSla,

                Technicians = technicians,
                Notes = baseDto.Notes ?? new List<MaintenanceNoteDTO>()
            };

            return (report, "Success");
        }

        public async Task<(byte[]? FileContent, string FileName, string ContentType, string MessageKey)> GetSingleRequestPdfAsync(
            int requestId,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default)
        {
            var (report, msg) = await GetSingleRequestAsync(requestId, userId, userRole, language, ct);
            if (report is null)
                return (null, string.Empty, string.Empty, msg);

            var document = new SingleRequestReportDocument(report);
            var bytes = document.GeneratePdf();

            var fileName = $"MaintenanceRequest_{report.RequestId}.pdf";
            return (bytes, fileName, "application/pdf", "Success");
        }

        public async Task<(PeriodRequestsReportDTO Report, string MessageKey)> GetRequestsPeriodAsync(
    DateTime fromUtc,
    DateTime toUtc,
    int? problemTypeId,
    string userId,
    string userRole,
    string language = "ar",
    CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var isAdmin = string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase);
            var isManager = string.Equals(userRole, "MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isEmployee = string.Equals(userRole, "Employee", StringComparison.OrdinalIgnoreCase);
            var isTechnician = string.Equals(userRole, "Technician", StringComparison.OrdinalIgnoreCase);

            // نضمن أن toUtc أكبر من fromUtc
            if (toUtc <= fromUtc)
                toUtc = fromUtc.AddDays(1);

            // نشتغل على CreatedAt ضمن الفترة
            var query = _requestRepository.Query(
                asTracking: false,
                include: q => q
                    .Include(r => r.ProblemType)
                        .ThenInclude(pt => pt.Translations)
                    .Include(r => r.Technicians),
                predicate: r =>
                    r.CreatedAt >= fromUtc &&
                    r.CreatedAt <= toUtc &&
                    r.Status == DAL.Entities.Status.Active);

            if (problemTypeId.HasValue)
                query = query.Where(r => r.ProblemTypeId == problemTypeId.Value);

            // صلاحيات حسب الدور
            query = query.Where(r =>
                isAdmin ||
                isManager ||
                (isEmployee && r.OwnerUserId == userId) ||
                (isTechnician && (r.CreatedByUserId == userId ||
                                  r.Technicians.Any(t => t.TechnicianUserId == userId && t.UnassignedAtUtc == null))));

            var entities = await query.ToListAsync(ct);

            var now = DateTime.UtcNow;

            var items = new List<PeriodRequestsReportItemDTO>();

            foreach (var entity in entities)
            {
                // نستخدم المابر للحصول على أسماء الحالة والنوع مترجمة
                var baseDto = MaintenanceRequestMapper.ToResponse(
                    entity,
                    userRole,
                    _fileService.GetPublicUrl,
                    language,
                    isOwner: false); // هنا مش مهم isOwner في التقرير الإداري

                // الفني الرئيسي = أول فني تم تعيينه
                var firstTechLink = entity.Technicians
                    .OrderBy(t => t.AssignedAtUtc)
                    .FirstOrDefault();

                string? mainTechnicianName = null;
                if (firstTechLink != null)
                {
                    var techUser = await _userRepository.GetByIdAsync(firstTechLink.TechnicianUserId, ct);
                    mainTechnicianName = techUser?.FullName ?? firstTechLink.TechnicianUserId;
                }

                // SLA من أول ExpectedDuration موجود
                int? slaHours = entity.Technicians
                    .Where(t => t.ExpectedDuration.HasValue)
                    .OrderBy(t => t.AssignedAtUtc)
                    .Select(t => t.ExpectedDuration)
                    .FirstOrDefault();

                bool? isWithinSla = null;
                bool isOverdue = false;

                if (slaHours.HasValue)
                {
                    var slaDuration = TimeSpan.FromHours(slaHours.Value);
                    var elapsedIfClosed = (entity.ClosedAtUtc ?? now) - entity.CreatedAt;

                    if (entity.ClosedAtUtc is not null)
                    {
                        isWithinSla = elapsedIfClosed <= slaDuration;
                        isOverdue = elapsedIfClosed > slaDuration;
                    }
                    else
                    {
                        // طلب مفتوح: متأخر لو المدة الحالية > SLA
                        isOverdue = elapsedIfClosed > slaDuration;
                    }
                }

                items.Add(new PeriodRequestsReportItemDTO
                {
                    RequestId = entity.Id,
                    CreatedAtUtc = entity.CreatedAt,
                    ProblemTypeName = baseDto.ProblemTypeName ?? "",
                    CaseTypeName = baseDto.CaseType,
                    MainTechnicianName = mainTechnicianName,
                    ClosedAtUtc = entity.ClosedAtUtc,
                    IsWithinSla = isWithinSla,
                    IsOverdue = isOverdue
                });
            }

            var summary = new PeriodRequestsReportSummaryDTO
            {
                TotalRequests = items.Count,
                CompletedCount = entities.Count(r => r.CaseType == DAL.Entities.MaintenanceRequestEntity.CaseType.Completed),
                CancelledCount = entities.Count(r => r.CaseType == DAL.Entities.MaintenanceRequestEntity.CaseType.Cancelled),
                OpenCount = entities.Count(r => r.CaseType != DAL.Entities.MaintenanceRequestEntity.CaseType.Completed &&
                                                r.CaseType != DAL.Entities.MaintenanceRequestEntity.CaseType.Cancelled),
                OverdueCount = items.Count(i => i.IsOverdue)
            };

            string? problemTypeName = null;
            if (problemTypeId.HasValue)
            {
                var any = entities.FirstOrDefault();
                if (any?.ProblemType?.Translations != null)
                {
                    problemTypeName = any.ProblemType.Translations
                        .FirstOrDefault(t => t.Language == language)?.Name
                        ?? any.ProblemType.Translations.FirstOrDefault()?.Name;
                }
            }

            var report = new PeriodRequestsReportDTO
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                ProblemTypeId = problemTypeId,
                ProblemTypeName = problemTypeName,
                Summary = summary,
                Items = items
            };

            return (report, "Success");
        }
        public async Task<(byte[]? FileContent, string FileName, string ContentType, string MessageKey)> GetRequestsPeriodPdfAsync(
            DateTime fromUtc,
            DateTime toUtc,
            int? problemTypeId,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default)
        {
            var (report, msg) = await GetRequestsPeriodAsync(fromUtc, toUtc, problemTypeId, userId, userRole, language, ct);

            // حتى لو مافي طلبات، بنرجع PDF فاضي لكن فيه Summary
            var document = new PeriodRequestsReportDocument(report);
            var bytes = document.GeneratePdf();

            var fileName = $"MaintenanceRequests_{fromUtc:yyyyMMdd}_{toUtc:yyyyMMdd}.pdf";
            return (bytes, fileName, "application/pdf", msg);
        }


        public async Task<(KpiRequestsReportDTO Report, string MessageKey)> GetKpiRequestsAsync(
    DateTime fromUtc,
    DateTime toUtc,
    int? problemTypeId,
    string userId,
    string userRole,
    string language = "ar",
    CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var isAdmin = string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase);
            var isManager = string.Equals(userRole, "MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isEmployee = string.Equals(userRole, "Employee", StringComparison.OrdinalIgnoreCase);
            var isTechnician = string.Equals(userRole, "Technician", StringComparison.OrdinalIgnoreCase);

            if (toUtc <= fromUtc)
                toUtc = fromUtc.AddDays(1);

            var query = _requestRepository.Query(
                asTracking: false,
                include: q => q
                    .Include(r => r.ProblemType)
                        .ThenInclude(pt => pt.Translations)
                    .Include(r => r.OwnerUser)
                    .Include(r => r.Technicians),
                predicate: r =>
                    r.CreatedAt >= fromUtc &&
                    r.CreatedAt <= toUtc &&
                    r.Status == DAL.Entities.Status.Active);

            if (problemTypeId.HasValue)
                query = query.Where(r => r.ProblemTypeId == problemTypeId.Value);

            query = query.Where(r =>
                isAdmin ||
                isManager ||
                (isEmployee && r.OwnerUserId == userId) ||
                (isTechnician && (r.CreatedByUserId == userId ||
                                  r.Technicians.Any(t => t.TechnicianUserId == userId && t.UnassignedAtUtc == null))));

            var entities = await query.ToListAsync(ct);
            var now = DateTime.UtcNow;

            var total = entities.Count;

            var closedEntities = entities.Where(r =>
                r.CaseType == DAL.Entities.MaintenanceRequestEntity.CaseType.Completed ||
                r.CaseType == DAL.Entities.MaintenanceRequestEntity.CaseType.Cancelled).ToList();

            var openEntities = entities.Except(closedEntities).ToList();

            // SLA per request
            int overdueCount = 0;
            int closedWithinSlaCount = 0;
            int closedWithSlaCount = 0;

            foreach (var r in entities)
            {
                int? slaHours = r.Technicians
                    .Where(t => t.ExpectedDuration.HasValue)
                    .OrderBy(t => t.AssignedAtUtc)
                    .Select(t => t.ExpectedDuration)
                    .FirstOrDefault();

                if (!slaHours.HasValue)
                    continue;

                var slaDuration = TimeSpan.FromHours(slaHours.Value);
                var end = r.ClosedAtUtc ?? now;
                var elapsed = end - r.CreatedAt;

                var isClosed = closedEntities.Contains(r);

                if (isClosed)
                {
                    closedWithSlaCount++;
                    if (elapsed <= slaDuration)
                    {
                        closedWithinSlaCount++;
                    }
                    else
                    {
                        overdueCount++;
                    }
                }
                else
                {
                    if (elapsed > slaDuration)
                        overdueCount++;
                }
            }

            // Average closure time (hours) للطلبات المغلقة
            double? avgClosureHours = null;
            var closedWithTime = closedEntities.Where(r => r.ClosedAtUtc.HasValue).ToList();
            if (closedWithTime.Count > 0)
            {
                avgClosureHours = closedWithTime
                    .Average(r => (r.ClosedAtUtc!.Value - r.CreatedAt).TotalHours);
            }

            var summary = new KpiRequestsSummaryDTO
            {
                TotalRequests = total,
                NewRequests = total, // حسب التعريف الحالي: كل الطلبات في الفترة هي "جديدة" ضمنها
                ClosedRequests = closedEntities.Count,
                OpenRequests = openEntities.Count,
                RemainingRequests = openEntities.Count,
                OverdueRequests = overdueCount
            };

            if (total > 0)
            {
                summary.CompletionRate = (double)summary.ClosedRequests / total * 100.0;
                summary.OverdueRate = (double)summary.OverdueRequests / total * 100.0;
            }

            if (closedWithSlaCount > 0)
            {
                summary.SlaComplianceRate = (double)closedWithinSlaCount / closedWithSlaCount * 100.0;
            }

            summary.AverageClosureHours = avgClosureHours;

            // Top Problem Types (Top 3)
            var topProblemTypes = entities
                .GroupBy(r => r.ProblemTypeId)
                .Select(g =>
                {
                    var any = g.First();
                    string name = string.Empty;
                    if (any.ProblemType?.Translations != null)
                    {
                        name = any.ProblemType.Translations
                            .FirstOrDefault(t => t.Language == language)?.Name
                            ?? any.ProblemType.Translations.FirstOrDefault()?.Name
                            ?? string.Empty;
                    }

                    return new KpiTopProblemTypeDTO
                    {
                        ProblemTypeId = g.Key,
                        ProblemTypeName = name,
                        Count = g.Count()
                    };
                })
                .OrderByDescending(x => x.Count)
                .Take(3)
                .ToList();

            // Top Departments (Top 3) من Department الخاص بالـ Owner
            var topDepartments = entities
                .Where(r => !string.IsNullOrWhiteSpace(r.OwnerUser?.Department))
                .GroupBy(r => r.OwnerUser!.Department!)
                .Select(g => new KpiTopDepartmentDTO
                {
                    DepartmentName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(3)
                .ToList();

            string? problemTypeName = null;
            if (problemTypeId.HasValue)
            {
                var any = entities.FirstOrDefault();
                if (any?.ProblemType?.Translations != null)
                {
                    problemTypeName = any.ProblemType.Translations
                        .FirstOrDefault(t => t.Language == language)?.Name
                        ?? any.ProblemType.Translations.FirstOrDefault()?.Name;
                }
            }

            var report = new KpiRequestsReportDTO
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                ProblemTypeId = problemTypeId,
                ProblemTypeName = problemTypeName,
                Summary = summary,
                TopProblemTypes = topProblemTypes,
                TopDepartments = topDepartments
            };

            return (report, "Success");
        }
        public async Task<(byte[]? FileContent, string FileName, string ContentType, string MessageKey)> GetKpiRequestsPdfAsync(
            DateTime fromUtc,
            DateTime toUtc,
            int? problemTypeId,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default)
        {
            var (report, msg) = await GetKpiRequestsAsync(fromUtc, toUtc, problemTypeId, userId, userRole, language, ct);

            var document = new KpiRequestsReportDocument(report);
            var bytes = document.GeneratePdf();

            var fileName = $"KpiRequests_{fromUtc:yyyyMMdd}_{toUtc:yyyyMMdd}.pdf";
            return (bytes, fileName, "application/pdf", msg);
        }
        public async Task<(DurationByProblemTypeReportDTO Report, string MessageKey)> GetDurationByProblemTypeAsync(
            DateTime fromUtc,
            DateTime toUtc,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var isAdmin = string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase);
            var isManager = string.Equals(userRole, "MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isEmployee = string.Equals(userRole, "Employee", StringComparison.OrdinalIgnoreCase);
            var isTechnician = string.Equals(userRole, "Technician", StringComparison.OrdinalIgnoreCase);

            if (toUtc <= fromUtc)
                toUtc = fromUtc.AddDays(1);

            // هنا نشتغل على الطلبات المكتملة/الملغاة التي أُغلِقت ضمن الفترة (ClosedAtUtc)
            var query = _requestRepository.Query(
                asTracking: false,
                include: q => q
                    .Include(r => r.ProblemType)
                        .ThenInclude(pt => pt.Translations)
                    .Include(r => r.OwnerUser)
                    .Include(r => r.Technicians),
                predicate: r =>
                    r.ClosedAtUtc != null &&
                    r.ClosedAtUtc >= fromUtc &&
                    r.ClosedAtUtc <= toUtc &&
                    r.Status == DAL.Entities.Status.Active &&
                    (
                        r.CaseType == DAL.Entities.MaintenanceRequestEntity.CaseType.Completed ||
                        r.CaseType == DAL.Entities.MaintenanceRequestEntity.CaseType.Cancelled
                    ));

            // صلاحيات
            query = query.Where(r =>
                isAdmin ||
                isManager ||
                (isEmployee && r.OwnerUserId == userId) ||
                (isTechnician && (r.CreatedByUserId == userId ||
                                  r.Technicians.Any(t => t.TechnicianUserId == userId && t.UnassignedAtUtc == null))));

            var entities = await query.ToListAsync(ct);
            var totalCompleted = entities.Count;

            // إعداد Buckets
            // إعداد Buckets (بسيطة: Key + Name فقط)
            // إعداد Buckets (بسيطة: Key + Name فقط)
            var buckets = new[]
            {
    new { Key = "lt12h",  Name = "أقل من 12 ساعة" },
    new { Key = "h12to72", Name = "من 12 ساعة إلى 3 أيام" },
    new { Key = "gt72h",  Name = "أكثر من 3 أيام" }
};

            var bucketCounts = buckets.ToDictionary(b => b.Key, _ => 0);



            // تصنيف الطلبات في البكيتات
            foreach (var r in entities)
            {
                if (!r.ClosedAtUtc.HasValue)
                    continue;

                var hours = (r.ClosedAtUtc.Value - r.CreatedAt).TotalHours;

                string? bucketKey = null;

                if (hours < 12)
                    bucketKey = "lt12h";
                else if (hours <= 72)
                    bucketKey = "h12to72";
                else
                    bucketKey = "gt72h";

                if (bucketKey != null)
                    bucketCounts[bucketKey]++;
            }

            var bucketDtos = new List<DurationBucketDTO>();
            foreach (var b in buckets)
            {
                var count = bucketCounts[b.Key];
                double percentage = 0;
                if (totalCompleted > 0)
                    percentage = (double)count / totalCompleted * 100.0;

                bucketDtos.Add(new DurationBucketDTO
                {
                    BucketKey = b.Key,
                    BucketName = b.Name,
                    Count = count,
                    Percentage = percentage
                });
            }

            // لكل نوع مشكلة: عدد مكتملة + متوسط زمن إغلاق + نسبة متأخرة
            var problemTypeDtos = new List<ProblemTypeDurationMetricsDTO>();

            var groups = entities.GroupBy(r => r.ProblemTypeId);
            foreach (var g in groups)
            {
                var list = g.ToList();
                var completedCount = list.Count;

                double? avgClosureHours = null;
                var withTime = list.Where(r => r.ClosedAtUtc.HasValue).ToList();
                if (withTime.Count > 0)
                {
                    avgClosureHours = withTime
                        .Average(r => (r.ClosedAtUtc!.Value - r.CreatedAt).TotalHours);
                }

                // حساب نسبة المتأخرة حسب SLA لكل نوع مشكلة
                int overdueWithSla = 0;
                int totalWithSla = 0;
                foreach (var r in list)
                {
                    int? slaHours = r.Technicians
                        .Where(t => t.ExpectedDuration.HasValue)
                        .OrderBy(t => t.AssignedAtUtc)
                        .Select(t => t.ExpectedDuration)
                        .FirstOrDefault();

                    if (!slaHours.HasValue || !r.ClosedAtUtc.HasValue)
                        continue;

                    totalWithSla++;

                    var slaDuration = TimeSpan.FromHours(slaHours.Value);
                    var elapsed = r.ClosedAtUtc.Value - r.CreatedAt;

                    if (elapsed > slaDuration)
                        overdueWithSla++;
                }

                double? overdueRate = null;
                if (totalWithSla > 0)
                    overdueRate = (double)overdueWithSla / totalWithSla * 100.0;

                // اسم نوع المشكلة مترجم
                string name = string.Empty;
                var any = list.FirstOrDefault();
                if (any?.ProblemType?.Translations != null)
                {
                    name = any.ProblemType.Translations
                        .FirstOrDefault(t => t.Language == language)?.Name
                        ?? any.ProblemType.Translations.FirstOrDefault()?.Name
                        ?? string.Empty;
                }

                problemTypeDtos.Add(new ProblemTypeDurationMetricsDTO
                {
                    ProblemTypeId = g.Key,
                    ProblemTypeName = name,
                    CompletedCount = completedCount,
                    AverageClosureHours = avgClosureHours,
                    OverdueRate = overdueRate
                });
            }

            var report = new DurationByProblemTypeReportDTO
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                TotalCompleted = totalCompleted,
                Buckets = bucketDtos,
                ProblemTypes = problemTypeDtos
            };

            return (report, "Success");
        }
        public async Task<(byte[]? FileContent, string FileName, string ContentType, string MessageKey)>
            GetDurationByProblemTypePdfAsync(
                DateTime fromUtc,
                DateTime toUtc,
                string userId,
                string userRole,
                string language = "ar",
                CancellationToken ct = default)
        {
            var (report, msg) = await GetDurationByProblemTypeAsync(
                fromUtc, toUtc, userId, userRole, language, ct);

            // حتى لو كان التقرير فاضي، يتم إصدار PDF
            var document = new DurationByProblemTypeReportDocument(report);
            var bytes = document.GeneratePdf();

            var fileName = $"DurationByProblemType_{fromUtc:yyyyMMdd}_{toUtc:yyyyMMdd}.pdf";

            return (bytes, fileName, "application/pdf", msg);
        }


    }
}
