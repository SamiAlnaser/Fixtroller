using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.MaintenanceRequestepository
{
    public interface IMaintenanceRequestTechnicianRepository
    {
        IQueryable<MaintenanceRequestTechnician> Query(bool asTracking = false);

        Task<bool> IsActiveAssignedAsync(int requestId, string technicianUserId, CancellationToken ct = default);
        Task<List<string>> GetActiveTechniciansAsync(int requestId, CancellationToken ct = default);

        Task AddActiveAsync(int requestId, string technicianUserId, CancellationToken ct = default);
        Task RemoveActiveAsync(int requestId, string technicianUserId, CancellationToken ct = default);
        Task SetActiveListAsync(int requestId, IEnumerable<string> technicianUserIds, CancellationToken ct = default);
    }
}
