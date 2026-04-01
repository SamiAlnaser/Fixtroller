using Fixtroller.BLL.Services.ReportsServices;
using Fixtroller.DAL.Data.DTOs.PagedResultDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.Reports.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Fixtroller.PL.Areas
{//
    [Route("Api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,MaintenanceManager,Employee,Technician")]
    public class ReportsController : ControllerBase
    {
        private readonly IMaintenanceReportsService _reportsService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ReportsController(
            IMaintenanceReportsService reportsService,
            IStringLocalizer<SharedResource> localizer)
        {
            _reportsService = reportsService;
            _localizer = localizer;
        }

        [HttpGet("Requests/{id:int}")]
        public async Task<IActionResult> GetSingleRequestReport(
            int id,
            [FromQuery] string? format,
            CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirst("role")?.Value
                     ?? User.FindFirstValue(ClaimTypes.Role)
                     ?? string.Empty;

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetSingleRequestPdfAsync(id, userId, role, language, ct);

                if (file is null)
                    return NotFound(new { message = _localizer[msgKey].Value });

                return File(file, contentType, fileName);
            }
            else
            {
                var (report, msgKey) =
                    await _reportsService.GetSingleRequestAsync(id, userId, role, language, ct);

                if (report is null)
                    return NotFound(new { message = _localizer[msgKey].Value });

                return Ok(new
                {
                    message = _localizer[msgKey].Value,
                    data = report
                });
            }
        }

        // تقرير الطلبات لفترة زمنية
        // GET: api/MaintenanceManager/Reports/requests-period?from=2025-01-01&to=2025-01-31&problemTypeId=1
        // GET: api/MaintenanceManager/Reports/requests-period?from=2025-01-01&to=2025-01-31&format=pdf
        [HttpGet("Requests-Period")]
        public async Task<IActionResult> GetRequestsPeriodReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? problemTypeId,
            [FromQuery] string? format,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirst("role")?.Value
                     ?? User.FindFirstValue(ClaimTypes.Role)
                     ?? string.Empty;

            // القيمة الافتراضية من 01-01-2025 في حال عدم إرسال from / to
            var fromValue = from ?? new DateTime(2025, 1, 1);
            var toValue = to ?? DateTime.UtcNow;

            // نتعامل مع from/to كـ UTC
            var fromUtc = DateTime.SpecifyKind(fromValue, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(toValue, DateTimeKind.Utc);

            // لو format = pdf => ما في باجينيشن (يرجع كل الداتا للـ PDF)
            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetRequestsPeriodPdfAsync(
                        fromUtc, toUtc, problemTypeId, userId, role, language, ct);

                if (file is null || file.Length == 0)
                    return NotFound(new { message = _localizer[msgKey].Value });

                return File(file, contentType, fileName);
            }

            // JSON + باجينيشن
            var (report, messageKey) =
                await _reportsService.GetRequestsPeriodAsync(
                    fromUtc, toUtc, problemTypeId, userId, role, language, ct);

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 5;

            var totalCount = report.Items?.Count ?? 0;
            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            var pageData = (report.Items ?? new List<PeriodRequestsReportItemDTO>())
                .OrderByDescending(i => i.CreatedAtUtc) // أو OrderBy لو بدك الأقدم أولاً
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedResult = new PagedResultDTO<PeriodRequestsReportItemDTO>
            {
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Data = pageData
            };

            return Ok(new
            {
                message = _localizer[messageKey].Value,
                data = new
                {
                    report.FromUtc,
                    report.ToUtc,
                    report.ProblemTypeId,
                    report.ProblemTypeName,
                    report.Summary,   // فيها إجمالي الطلبات، عدد المكتملة، المتأخرة... إلخ
                    Requests = pagedResult
                }
            });
        }

        // تقرير الأرقام العامة (KPI)
        // GET: api/MaintenanceManager/Reports/kpi-requests?from=2025-01-01&to=2025-01-31
        // GET: api/MaintenanceManager/Reports/kpi-requests?from=2025-01-01&to=2025-01-31&format=pdf
        [Authorize(Roles = "Admin,MaintenanceManager")]
        [HttpGet("Kpi-Requests")]
        public async Task<IActionResult> GetKpiRequestsReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? problemTypeId,
            [FromQuery] string? format,
            CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirst("role")?.Value
                     ?? User.FindFirstValue(ClaimTypes.Role)
                     ?? string.Empty;

            // القيمة الافتراضية من 01-01-2025 في حال عدم إرسال from / to
            var fromValue = from ?? new DateTime(2025, 1, 1);
            var toValue = to ?? DateTime.UtcNow;

            var fromUtc = DateTime.SpecifyKind(fromValue, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(toValue, DateTimeKind.Utc);

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetKpiRequestsPdfAsync(fromUtc, toUtc, problemTypeId, userId, role, language, ct);

                if (file is null || file.Length == 0)
                    return NotFound(new { message = _localizer[msgKey].Value });

                return File(file, contentType, fileName);
            }
            else
            {
                var (report, msgKey) =
                    await _reportsService.GetKpiRequestsAsync(fromUtc, toUtc, problemTypeId, userId, role, language, ct);

                return Ok(new
                {
                    message = _localizer[msgKey].Value,
                    data = report
                });
            }
        }

        // تقرير مدة الإغلاق حسب نوع المشكلة
        // GET: api/MaintenanceManager/Reports/duration-problem-types?from=2025-01-01&to=2025-01-31
        // GET: api/MaintenanceManager/Reports/duration-problem-types?from=2025-01-01&to=2025-01-31&format=pdf
        [Authorize(Roles = "Admin,MaintenanceManager")]
        [HttpGet("Duration-Problem-Types")]
        public async Task<IActionResult> GetDurationByProblemTypeReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? format,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirst("role")?.Value
                     ?? User.FindFirstValue(ClaimTypes.Role)
                     ?? string.Empty;

            // القيمة الافتراضية من 01-01-2025 في حال عدم إرسال from / to
            var fromValue = from ?? new DateTime(2025, 1, 1);
            var toValue = to ?? DateTime.UtcNow;

            var fromUtc = DateTime.SpecifyKind(fromValue, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(toValue, DateTimeKind.Utc);

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetDurationByProblemTypePdfAsync(fromUtc, toUtc, userId, role, language, ct);

                if (file is null || file.Length == 0)
                    return NotFound(new { message = _localizer[msgKey].Value });

                return File(file, contentType, fileName);
            }
            else
            {
                var (report, msgKey) =
                    await _reportsService.GetDurationByProblemTypeAsync(fromUtc, toUtc, userId, role, language, ct);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;

                var totalCount = report.ProblemTypes?.Count ?? 0;
                var totalPages = totalCount == 0
                    ? 0
                    : (int)Math.Ceiling(totalCount / (double)pageSize);

                var pageData = (report.ProblemTypes ?? new List<ProblemTypeDurationMetricsDTO>())
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagedResult = new PagedResultDTO<ProblemTypeDurationMetricsDTO>
                {
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    Data = pageData
                };

                return Ok(new
                {
                    message = _localizer[msgKey].Value,
                    data = new
                    {
                        report.FromUtc,
                        report.ToUtc,
                        report.TotalCompleted,
                        Buckets = report.Buckets,
                        ProblemTypes = pagedResult
                    }
                });
            }
        }


        // تقرير الفني الواحد
        // GET: api/MaintenanceManager/Reports/technicians/tech-001?from=2025-01-01&to=2025-01-31
        // GET: api/MaintenanceManager/Reports/technicians/tech-001?from=2025-01-01&to=2025-01-31&format=pdf
        [Authorize(Roles = "Admin,MaintenanceManager")]
        [HttpGet("Technicians/{technicianUserId}")]
        public async Task<IActionResult> GetTechnicianPerformanceReport(
    string technicianUserId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromQuery] string? format,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirst("role")?.Value
                     ?? User.FindFirstValue(ClaimTypes.Role)
                     ?? string.Empty;

            // القيمة الافتراضية من 01-01-2025 في حال عدم إرسال from / to
            var fromValue = from ?? new DateTime(2025, 1, 1);
            var toValue = to ?? DateTime.UtcNow;

            var fromUtc = DateTime.SpecifyKind(fromValue, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(toValue, DateTimeKind.Utc);

            // PDF بدون باجينيشن
            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetTechnicianPerformancePdfAsync(
                        technicianUserId, fromUtc, toUtc, userId, role, language, ct);

                if (msgKey == "Forbidden" || msgKey == "User_NotFound")
                    return BadRequest(new { message = _localizer[msgKey].Value });

                if (file is null || file.Length == 0)
                    return NotFound(new { message = _localizer[msgKey].Value });

                return File(file, contentType, fileName);
            }

            // JSON + باجينيشن
            var (report, messageKey) =
                await _reportsService.GetTechnicianPerformanceAsync(
                    technicianUserId, fromUtc, toUtc, userId, role, language, ct);

            if (messageKey == "Forbidden" || messageKey == "User_NotFound")
                return BadRequest(new { message = _localizer[messageKey].Value });

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var totalCount = report.Items?.Count ?? 0;
            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            var pageData = (report.Items ?? new List<TechnicianRequestPerformanceItemDTO>())
                .OrderByDescending(i => i.CreatedAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedResult = new PagedResultDTO<TechnicianRequestPerformanceItemDTO>
            {
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Data = pageData
            };

            return Ok(new
            {
                message = _localizer[messageKey].Value,
                data = new
                {
                    report.TechnicianUserId,
                    report.TechnicianName,
                    report.TechnicianCategoryName,
                    report.FromUtc,
                    report.ToUtc,
                    report.Summary,   // إحصائيات الفني (كم طلب، كم اكتمل، معدل وقت الإغلاق...)
                    Requests = pagedResult
                }
            });
        }


        // تقرير الفنيين حسب الـ Category
        // GET: api/MaintenanceManager/Reports/technicians-by-category?from=2025-01-01&to=2025-01-31
        // GET: api/MaintenanceManager/Reports/technicians-by-category?from=2025-01-01&to=2025-01-31&format=pdf
        [Authorize(Roles = "Admin,MaintenanceManager")]
        [HttpGet("Technicians-By-Category")]
        public async Task<IActionResult> GetTechniciansByCategoryReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? format,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirst("role")?.Value
                     ?? User.FindFirstValue(ClaimTypes.Role)
                     ?? string.Empty;

            // القيمة الافتراضية من 01-01-2025 في حال عدم إرسال from / to
            var fromValue = from ?? new DateTime(2025, 1, 1);
            var toValue = to ?? DateTime.UtcNow;

            var fromUtc = DateTime.SpecifyKind(fromValue, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(toValue, DateTimeKind.Utc);

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetTechnicianCategoriesPerformancePdfAsync(
                        fromUtc, toUtc, userId, role, language, ct);

                if (msgKey == "Forbidden")
                    return BadRequest(new { message = _localizer[msgKey].Value });

                if (file is null || file.Length == 0)
                    return NotFound(new { message = _localizer[msgKey].Value });

                return File(file, contentType, fileName);
            }
            else
            {
                var (report, msgKey) =
                    await _reportsService.GetTechnicianCategoriesPerformanceAsync(
                        fromUtc, toUtc, userId, role, language, ct);

                if (msgKey == "Forbidden")
                    return BadRequest(new { message = _localizer[msgKey].Value });

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;

                // نفرد الفنيين لكل Category في جدول واحد للـ باجينيشن
                var flatRows = (report.Categories ?? new List<TechnicianCategoryPerformanceDTO>())
                    .SelectMany(cat => cat.Technicians.Select(t => new
                    {
                        CategoryId = cat.CategoryId,
                        CategoryName = cat.CategoryName,
                        cat.TotalAssigned,
                        cat.TotalCompleted,
                        cat.TotalOverdue,
                        cat.CompletionRate,
                        cat.OverdueRate,
                        cat.TechniciansCount,
                        CategoryAverageClosureHours = cat.AverageClosureHours,
                        cat.AverageRequestsPerTechnician,
                        t.TechnicianUserId,
                        t.TechnicianName,
                        t.AssignedCount,
                        t.CompletedCount,
                        t.OverdueCount,
                        TechnicianAverageClosureHours = t.AverageClosureHours
                    }))
                    .ToList();

                var totalCount = flatRows.Count;
                var totalPages = totalCount == 0
                    ? 0
                    : (int)Math.Ceiling(totalCount / (double)pageSize);

                var pageData = flatRows
                    .OrderByDescending(r => r.CompletedCount)
                    .ThenBy(r => r.CategoryName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagedResult = new PagedResultDTO<object>
                {
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    Data = pageData.Cast<object>().ToList()
                };

                return Ok(new
                {
                    message = _localizer[msgKey].Value,
                    data = new
                    {
                        report.FromUtc,
                        report.ToUtc,
                        // بإمكان الفرونت استخدام الـ Categories للملخصات / المخططات
                        report.Categories,
                        Technicians = pagedResult
                    }
                });
            }
        }
        // تقرير قسم الصيانة ككل
        // GET: api/MaintenanceManager/Reports/maintenance-department?from=2025-01-01&to=2025-01-31
        // GET: api/MaintenanceManager/Reports/maintenance-department?from=2025-01-01&to=2025-01-31&format=pdf
        [Authorize(Roles = "Admin,MaintenanceManager")]
        [HttpGet("Maintenance-Department")]
        public async Task<IActionResult> GetMaintenanceDepartmentReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? format,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirst("role")?.Value
                     ?? User.FindFirstValue(ClaimTypes.Role)
                     ?? string.Empty;

            // القيمة الافتراضية من 01-01-2025 في حال عدم إرسال from / to
            var fromValue = from ?? new DateTime(2025, 1, 1);
            var toValue = to ?? DateTime.UtcNow;

            var fromUtc = DateTime.SpecifyKind(fromValue, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(toValue, DateTimeKind.Utc);

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetMaintenanceDepartmentPdfAsync(
                        fromUtc, toUtc, userId, role, language, ct);

                if (msgKey == "Forbidden")
                    return BadRequest(new { message = _localizer[msgKey].Value });

                if (file is null || file.Length == 0)
                    return NotFound(new { message = _localizer[msgKey].Value });

                return File(file, contentType, fileName);
            }
            else
            {
                var (report, msgKey) =
                    await _reportsService.GetMaintenanceDepartmentAsync(
                        fromUtc, toUtc, userId, role, language, ct);

                if (msgKey == "Forbidden")
                    return BadRequest(new { message = _localizer[msgKey].Value });

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;

                var totalCount = report.Categories?.Count ?? 0;
                var totalPages = totalCount == 0
                    ? 0
                    : (int)Math.Ceiling(totalCount / (double)pageSize);

                var pageData = (report.Categories ?? new List<MaintenanceDepartmentCategoryStatDTO>())
                    .OrderByDescending(c => c.RequestsCount)
                    .ThenBy(c => c.CategoryName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagedResult = new PagedResultDTO<MaintenanceDepartmentCategoryStatDTO>
                {
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    Data = pageData
                };

                return Ok(new
                {
                    message = _localizer[msgKey].Value,
                    data = new
                    {
                        report.FromUtc,
                        report.ToUtc,
                        report.Summary,
                        report.TotalTechnicians,
                        Categories = pagedResult,
                        report.TopProblemTypes,
                        report.TopCategoriesByRequests
                    }
                });
            }
        }

    }
}