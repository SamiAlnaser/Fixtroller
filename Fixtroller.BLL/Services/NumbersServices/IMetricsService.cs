using Fixtroller.DAL.Data.DTOs.NumbersDTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.NumbersServices
{
    public interface IMetricsService
    {
        Task<TechnicianNumbersDTO> GetTechnicianNumbersAsync(
            string technicianUserId,
            CancellationToken ct = default);

        Task<ManagerDashboardNumbersDTO> GetManagerDashboardAsync(CancellationToken ct = default);

        Task<TechnicianDashboardNumbersDTO> GetTechnicianDashboardAsync(string technicianUserId, CancellationToken ct = default);

        Task<EmployeeDashboardNumbersDTO> GetEmployeeDashboardAsync(string employeeUserId, CancellationToken ct = default);

        Task<List<ChartPointDTO>> GetRequestsByTechnicianCategoryAsync(
        string language = "ar",
        DateTimeOffset? fromUtc = null,
        DateTimeOffset? toUtc = null,
        CancellationToken ct = default);

        Task<List<StatusDistributionDTO>> GetStatusDistributionAsync(
            string language = "ar",
            DateTimeOffset? fromUtc = null,
            DateTimeOffset? toUtc = null,
            CancellationToken ct = default);
        Task<ManagerChartsDTO> GetManagerChartsAsync(
    string language = "ar",
    DateTimeOffset? fromUtc = null,
    DateTimeOffset? toUtc = null,
    CancellationToken ct = default);
    }
}

