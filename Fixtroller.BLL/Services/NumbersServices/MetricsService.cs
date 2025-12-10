using Fixtroller.BLL.Mapping;
using Fixtroller.DAL.Data.DTOs.NumbersDTOs.Responses;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.NumbersRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.NumbersServices
{
    public class MetricsService : IMetricsService
    {
        private readonly IMetricsRepository _repo;

        public MetricsService(IMetricsRepository repo)
            => _repo = repo;

        public async Task<TechnicianNumbersDTO> GetTechnicianNumbersAsync(
            string technicianUserId,
            CancellationToken ct = default)
        {
            var currentTasks = await _repo.CountActiveRequestsForTechnicianAsync(technicianUserId, ct);
            var avgHours = await _repo.AverageCompletionHoursForTechnicianAsync(technicianUserId, ct);

            return new TechnicianNumbersDTO
            {
                CurrentTasks = currentTasks,
                AverageCompletionHours = Math.Round(avgHours, 2)
            };
        }

        public async Task<ManagerDashboardNumbersDTO> GetManagerDashboardAsync(CancellationToken ct = default)
        {
            var total = await _repo.CountAllRequestsAsync(ct);
            var proc = await _repo.CountRequestsByCasesAsync(new[] { CaseType.Processing }, ct);
            var done = await _repo.CountRequestsByCasesAsync(new[] { CaseType.Completed }, ct);
            var newReqs = await _repo.CountRequestsByCasesAsync(new[] { CaseType.Submitted }, ct);
            var needRes = await _repo.CountRequestsByCasesAsync(new[] { CaseType.ResourcesNeeded }, ct);

            return new ManagerDashboardNumbersDTO
            {
                TotalRequests = total,
                Processing = proc,
                Completed = done,
                Submitted = newReqs,
                ResourcesNeeded = needRes
            };
        }

        public async Task<TechnicianDashboardNumbersDTO> GetTechnicianDashboardAsync(
    string technicianUserId, CancellationToken ct = default)
        {
            var newCount = await _repo.CountNewRequestsForTechnicianAsync(technicianUserId, ct);
            var procCount = await _repo.CountRequestsByCasesForTechnicianAsync(
                                technicianUserId, new[] { CaseType.Processing }, ct);
            var doneCount = await _repo.CountRequestsByCasesForTechnicianAsync(
                                technicianUserId, new[] { CaseType.Completed }, ct);

            return new TechnicianDashboardNumbersDTO
            {
                NewRequests = newCount,
                Processing = procCount,
                Completed = doneCount
            };
        }

        public async Task<EmployeeDashboardNumbersDTO> GetEmployeeDashboardAsync(
       string employeeUserId,
       CancellationToken ct = default)
        {
            var total = await _repo.CountAllRequestsForOwnerAsync(employeeUserId, ct);

            // قيد الانتظار (تعريف مرن: Submitted + ManagerReview + ResourcesNeeded)
            var waiting = await _repo.CountRequestsByCasesForOwnerAsync(
                employeeUserId, new[] { CaseType.Submitted, CaseType.ManagerReview, CaseType.ResourcesNeeded }, ct);

            var processing = await _repo.CountRequestsByCasesForOwnerAsync(
                employeeUserId, new[] { CaseType.Processing }, ct);

            var completed = await _repo.CountRequestsByCasesForOwnerAsync(
                employeeUserId, new[] { CaseType.Completed }, ct);

            var cancelled = await _repo.CountRequestsByCasesForOwnerAsync(
                employeeUserId, new[] { CaseType.Cancelled }, ct);

            return new EmployeeDashboardNumbersDTO
            {
                Total = total,
                Waiting = waiting,
                Processing = processing,
                Completed = completed,
                Cancelled = cancelled
            };
        }

        public async Task<List<ChartPointDTO>> GetRequestsByTechnicianCategoryAsync(
        string language = "ar",
        DateTimeOffset? fromUtc = null,
        DateTimeOffset? toUtc = null,
        CancellationToken ct = default)
        {
            var rows = await _repo.GetRequestsByTechnicianCategoryAsync(language, fromUtc, toUtc, ct);
            return rows.Select(x => new ChartPointDTO
            {
                Label = x.Name,
                Count = x.Count
            }).ToList();
        }

        public async Task<List<StatusDistributionDTO>> GetStatusDistributionAsync(
            string language = "ar",
            DateTimeOffset? fromUtc = null,
            DateTimeOffset? toUtc = null,
            CancellationToken ct = default)
        {
            var rows = await _repo.GetRequestsStatusDistributionAsync(fromUtc, toUtc, ct);
            return rows.Select(x => new StatusDistributionDTO
            {
                CaseType = MaintenanceRequestMapper.GetCaseTypeName(x.Case, language),
                Count = x.Count
            }).ToList();
        }

        public async Task<ManagerChartsDTO> GetManagerChartsAsync(
            string language = "ar",
            DateTimeOffset? fromUtc = null,
            DateTimeOffset? toUtc = null,
            CancellationToken ct = default)
        {
            // تنفيذ تسلسلي لتفادي مشكلة الـ DbContext concurrent operations
            var requestsByCategory = await GetRequestsByTechnicianCategoryAsync(language, fromUtc, toUtc, ct);
            var statusDistribution = await GetStatusDistributionAsync(language, fromUtc, toUtc, ct);

            return new ManagerChartsDTO
            {
                RequestsByCategory = requestsByCategory,
                StatusDistribution = statusDistribution
            };
        }
    }
}
