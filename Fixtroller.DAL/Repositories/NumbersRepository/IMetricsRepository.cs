using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.NumbersRepository
{
    public interface IMetricsRepository
    {
        // عدد الطلبات “النشطة” لفنّي (حالات غير نهائية)
        Task<int> CountActiveRequestsForTechnicianAsync(
            string technicianUserId,
            CancellationToken ct = default);

        // متوسط زمن الإنجاز (بالساعات) للطلبات المكتملة لهذا الفنّي
        Task<double> AverageCompletionHoursForTechnicianAsync(
            string technicianUserId,
            CancellationToken ct = default);

        Task<int> CountRequestsByCasesForTechnicianAsync(
    string technicianUserId,
    IEnumerable<CaseType> cases,
    CancellationToken ct = default);
        Task<double> SumWorkSecondsForTechnicianAsync(
            string technicianUserId,
            DateTimeOffset? fromUtc = null,
            DateTimeOffset? toUtc = null,
            CancellationToken ct = default);

        Task<int> CountAllRequestsAsync(CancellationToken ct = default);
        Task<int> CountRequestsByCasesAsync(IEnumerable<CaseType> cases, CancellationToken ct = default);
    
        Task<int> CountNewRequestsForTechnicianAsync(string technicianUserId, CancellationToken ct = default);

        Task<int> CountAllRequestsForOwnerAsync(string ownerUserId, CancellationToken ct = default);
        Task<int> CountRequestsByCasesForOwnerAsync(string ownerUserId, IEnumerable<CaseType> cases, CancellationToken ct = default);

        Task<List<(int CategoryId, string Name, int Count)>> GetRequestsByTechnicianCategoryAsync(
    string language = "ar",
    DateTimeOffset? fromUtc = null,
    DateTimeOffset? toUtc = null,
    CancellationToken ct = default);

        Task<List<(CaseType Case, int Count)>> GetRequestsStatusDistributionAsync(
            DateTimeOffset? fromUtc = null,
            DateTimeOffset? toUtc = null,
            CancellationToken ct = default);

    }
}

