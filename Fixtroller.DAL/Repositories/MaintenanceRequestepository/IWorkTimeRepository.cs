using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.MaintenanceRequestepository
{
    public interface IWorkTimeRepository
    {
        IQueryable<WorkTimeEntry> Query(bool asTracking = false);

        Task StartAsync(WorkTimeEntry entry, CancellationToken ct = default);
        Task<bool> HasActiveAsync(int requestId, string technicianUserId, CancellationToken ct = default);

        Task StopActiveForRequestAsync(int requestId, CancellationToken ct = default);
        Task StopActiveForRequestAndTechAsync(int requestId, string technicianUserId, CancellationToken ct = default);
    }
}
