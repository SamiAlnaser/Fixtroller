using Fixtroller.DAL.Data.DTOs.Reports.Responses;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.ReportsServices
{
    public interface IMaintenanceReportsService
    {
        // تقرير الطلب الواحد
        Task<(SingleRequestReportDTO? Report, string MessageKey)> GetSingleRequestAsync(
            int requestId,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default);

        Task<(byte[]? FileContent, string FileName, string ContentType, string MessageKey)> GetSingleRequestPdfAsync(
            int requestId,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default);

        // تقرير الطلبات لفترة
        Task<(PeriodRequestsReportDTO Report, string MessageKey)> GetRequestsPeriodAsync(
            DateTime fromUtc,
            DateTime toUtc,
            int? problemTypeId,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default);

        Task<(byte[]? FileContent, string FileName, string ContentType, string MessageKey)> GetRequestsPeriodPdfAsync(
            DateTime fromUtc,
            DateTime toUtc,
            int? problemTypeId,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default);

        // تقرير الأرقام العامة (KPI)
        Task<(KpiRequestsReportDTO Report, string MessageKey)> GetKpiRequestsAsync(
            DateTime fromUtc,
            DateTime toUtc,
            int? problemTypeId,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default);

        Task<(byte[]? FileContent, string FileName, string ContentType, string MessageKey)> GetKpiRequestsPdfAsync(
            DateTime fromUtc,
            DateTime toUtc,
            int? problemTypeId,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default);

        // تقرير التصنيفات حسب المدة ونوع المشكلة
        Task<(DurationByProblemTypeReportDTO Report, string MessageKey)> GetDurationByProblemTypeAsync(
            DateTime fromUtc,
            DateTime toUtc,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default);

        Task<(byte[]? FileContent, string FileName, string ContentType, string MessageKey)> GetDurationByProblemTypePdfAsync(
            DateTime fromUtc,
            DateTime toUtc,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default);
        // تقرير الفني الواحد
        Task<(TechnicianPerformanceReportDTO Report, string MessageKey)> GetTechnicianPerformanceAsync(
            string technicianUserId,
            DateTime fromUtc,
            DateTime toUtc,
            string callerUserId,
            string callerRole,
            string language = "ar",
            CancellationToken ct = default);

        Task<(byte[]? FileContent, string FileName, string ContentType, string MessageKey)> GetTechnicianPerformancePdfAsync(
            string technicianUserId,
            DateTime fromUtc,
            DateTime toUtc,
            string callerUserId,
            string callerRole,
            string language = "ar",
            CancellationToken ct = default);

        // تقرير الفنيين حسب الـ Category
        Task<(TechnicianCategoriesPerformanceReportDTO Report, string MessageKey)> GetTechnicianCategoriesPerformanceAsync(
            DateTime fromUtc,
            DateTime toUtc,
            string callerUserId,
            string callerRole,
            string language = "ar",
            CancellationToken ct = default);

        Task<(byte[]? FileContent, string FileName, string ContentType, string MessageKey)> GetTechnicianCategoriesPerformancePdfAsync(
            DateTime fromUtc,
            DateTime toUtc,
            string callerUserId,
            string callerRole,
            string language = "ar",
            CancellationToken ct = default);
        // تقرير قسم الصيانة ككل
        Task<(MaintenanceDepartmentReportDTO Report, string MessageKey)> GetMaintenanceDepartmentAsync(
            DateTime fromUtc,
            DateTime toUtc,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default);

        Task<(byte[]? FileContent, string FileName, string ContentType, string MessageKey)> GetMaintenanceDepartmentPdfAsync(
            DateTime fromUtc,
            DateTime toUtc,
            string userId,
            string userRole,
            string language = "ar",
            CancellationToken ct = default);

    }
}
