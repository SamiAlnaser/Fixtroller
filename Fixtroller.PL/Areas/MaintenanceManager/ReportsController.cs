using Fixtroller.BLL.Services.ReportsServices;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Fixtroller.PL.Areas.MaintenanceManager.Controllers
{
    [Area("MaintenanceManager")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize(Roles = "MaintenanceManager,Admin")]
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

        // تقرير الطلب الواحد (زي ما عملناه قبل)
        [HttpGet("Requests/{id:int}")]
        public async Task<IActionResult> GetSingleRequestReport(
            int id,
            [FromQuery] string? format,
            CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

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
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int? problemTypeId,
            [FromQuery] string? format,
            CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            // نفترض أن from/to جايين كتاريخ (بدون timezone) ونتعامل معهم كـ UTC
            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetRequestsPeriodPdfAsync(fromUtc, toUtc, problemTypeId, userId, role, language, ct);

                return File(file, contentType, fileName);
            }
            else
            {
                var (report, msgKey) =
                    await _reportsService.GetRequestsPeriodAsync(fromUtc, toUtc, problemTypeId, userId, role, language, ct);

                return Ok(new
                {
                    message = _localizer[msgKey].Value,
                    data = report
                });
            }
        }


        // تقرير الأرقام العامة (KPI)
        // GET: api/MaintenanceManager/Reports/kpi-requests?from=2025-01-01&to=2025-01-31
        // GET: api/MaintenanceManager/Reports/kpi-requests?from=2025-01-01&to=2025-01-31&format=pdf
        [HttpGet("Kpi-Requests")]
        public async Task<IActionResult> GetKpiRequestsReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int? problemTypeId,
            [FromQuery] string? format,
            CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetKpiRequestsPdfAsync(fromUtc, toUtc, problemTypeId, userId, role, language, ct);

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
        // تقرير التصنيفات حسب مدة الإغلاق ونوع المشكلة
        // GET: api/MaintenanceManager/Reports/duration-problem-types?from=2025-01-01&to=2025-01-31
        // GET: api/MaintenanceManager/Reports/duration-problem-types?from=2025-01-01&to=2025-01-31&format=pdf
        [HttpGet("Duration-Problem-Types")]
        public async Task<IActionResult> GetDurationByProblemTypeReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string? format,
            CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetDurationByProblemTypePdfAsync(fromUtc, toUtc, userId, role, language, ct);

                return File(file, contentType, fileName);
            }
            else
            {
                var (report, msgKey) =
                    await _reportsService.GetDurationByProblemTypeAsync(fromUtc, toUtc, userId, role, language, ct);

                return Ok(new
                {
                    message = _localizer[msgKey].Value,
                    data = report
                });
            }
        }


        // تقرير الفني الواحد
        // GET: api/MaintenanceManager/Reports/technicians/tech-001?from=2025-01-01&to=2025-01-31
        // GET: api/MaintenanceManager/Reports/technicians/tech-001?from=2025-01-01&to=2025-01-31&format=pdf
        [HttpGet("technicians/{technicianUserId}")]
        public async Task<IActionResult> GetTechnicianPerformanceReport(
            string technicianUserId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string? format,
            CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var callerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var callerRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetTechnicianPerformancePdfAsync(
                        technicianUserId, fromUtc, toUtc, callerUserId, callerRole, language, ct);

                if (msgKey == "Forbidden" || msgKey == "User_NotFound")
                    return BadRequest(new { message = _localizer[msgKey].Value });

                return File(file, contentType, fileName);
            }
            else
            {
                var (report, msgKey) =
                    await _reportsService.GetTechnicianPerformanceAsync(
                        technicianUserId, fromUtc, toUtc, callerUserId, callerRole, language, ct);

                if (msgKey == "Forbidden" || msgKey == "User_NotFound")
                    return BadRequest(new { message = _localizer[msgKey].Value });

                return Ok(new
                {
                    message = _localizer[msgKey].Value,
                    data = report
                });
            }
        }


        // تقرير الفنيين حسب الـ Category
        // GET: api/MaintenanceManager/Reports/technicians-by-category?from=2025-01-01&to=2025-01-31
        // GET: api/MaintenanceManager/Reports/technicians-by-category?from=2025-01-01&to=2025-01-31&format=pdf
        [HttpGet("technicians-by-category")]
        public async Task<IActionResult> GetTechniciansByCategoryReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string? format,
            CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var callerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var callerRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetTechnicianCategoriesPerformancePdfAsync(
                        fromUtc, toUtc, callerUserId, callerRole, language, ct);

                if (msgKey == "Forbidden")
                    return BadRequest(new { message = _localizer[msgKey].Value });

                return File(file, contentType, fileName);
            }
            else
            {
                var (report, msgKey) =
                    await _reportsService.GetTechnicianCategoriesPerformanceAsync(
                        fromUtc, toUtc, callerUserId, callerRole, language, ct);

                if (msgKey == "Forbidden")
                    return BadRequest(new { message = _localizer[msgKey].Value });

                return Ok(new
                {
                    message = _localizer[msgKey].Value,
                    data = report
                });
            }
        }
        // تقرير قسم الصيانة ككل
        // GET: api/MaintenanceManager/Reports/maintenance-department?from=2025-01-01&to=2025-01-31
        // GET: api/MaintenanceManager/Reports/maintenance-department?from=2025-01-01&to=2025-01-31&format=pdf
        [HttpGet("maintenance-department")]
        public async Task<IActionResult> GetMaintenanceDepartmentReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string? format,
            CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var (file, fileName, contentType, msgKey) =
                    await _reportsService.GetMaintenanceDepartmentPdfAsync(
                        fromUtc, toUtc, userId, role, language, ct);

                if (msgKey == "Forbidden")
                    return BadRequest(new { message = _localizer[msgKey].Value });

                return File(file, contentType, fileName);
            }
            else
            {
                var (report, msgKey) =
                    await _reportsService.GetMaintenanceDepartmentAsync(
                        fromUtc, toUtc, userId, role, language, ct);

                if (msgKey == "Forbidden")
                    return BadRequest(new { message = _localizer[msgKey].Value });

                return Ok(new
                {
                    message = _localizer[msgKey].Value,
                    data = report
                });
            }
        }

    }
}