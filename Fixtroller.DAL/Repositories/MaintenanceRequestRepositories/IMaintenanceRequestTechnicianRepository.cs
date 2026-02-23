using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.MaintenanceRequestRepositories
{
    public interface IMaintenanceRequestTechnicianRepository
    {
        IQueryable<MaintenanceRequestTechnician> Query(bool asTracking = false);

        Task<bool> IsActiveAssignedAsync(int requestId, string technicianUserId, CancellationToken ct = default);
        Task<List<string>> GetActiveTechniciansAsync(int requestId, CancellationToken ct = default);

        Task AddActiveAsync(int requestId, string technicianUserId, int? expectedDuration, CancellationToken ct = default);
        Task RemoveActiveAsync(int requestId, string technicianUserId, CancellationToken ct = default);
        Task SetActiveListAsync(int requestId, IEnumerable<string> technicianUserIds, int? expectedDuration, CancellationToken ct = default);
        Task<bool> IsLeadAsync(int requestId, string technicianUserId, CancellationToken ct = default);

        Task<List<MaintenanceRequestTechnician>> GetActiveTechniciansWithStatusAsync(
            int requestId,
            CancellationToken ct = default);

        Task UpdateTechnicianStatusAsync(
            int requestId,
            string technicianUserId,
            TechnicianTaskStatus status,
            CancellationToken ct = default);

        Task SetLeadAsync(
            int requestId,
            string technicianUserId,
            CancellationToken ct = default);

        Task SetTaskGroupAsync(
    int requestId,
    IEnumerable<string> technicianUserIds,
    string taskGroupKey,
    string? leadTechnicianUserId,
    CancellationToken ct = default);


    }
}
