using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.MaintenanceRequestepository
{
    public sealed class WorkTimeRepository : IWorkTimeRepository
    {
        private readonly ApplicationDbContext _context;
        public WorkTimeRepository(ApplicationDbContext context) => _context = context;

        public IQueryable<WorkTimeEntry> Query(bool asTracking = false)
            => asTracking ? _context.Set<WorkTimeEntry>() : _context.Set<WorkTimeEntry>().AsNoTracking();

        public Task StartAsync(WorkTimeEntry entry, CancellationToken ct = default)
        {
            _context.Set<WorkTimeEntry>().Add(entry);
            return Task.CompletedTask;
        }

        public Task<bool> HasActiveAsync(int requestId, string technicianUserId, CancellationToken ct = default)
            => _context.Set<WorkTimeEntry>()
                  .AnyAsync(w => w.RequestId == requestId &&
                                 w.TechnicianUserId == technicianUserId &&
                                 w.StoppedAt == null, ct);

        public async Task StopActiveForRequestAsync(int requestId, CancellationToken ct = default)
        {
            var actives = await _context.Set<WorkTimeEntry>()
                                   .Where(w => w.RequestId == requestId && w.StoppedAt == null)
                                   .ToListAsync(ct);

            var now = System.DateTimeOffset.UtcNow;
            foreach (var w in actives)
                w.StoppedAt = now;
        }

        public async Task StopActiveForRequestAndTechAsync(int requestId, string technicianUserId, CancellationToken ct = default)
        {
            var actives = await _context.Set<WorkTimeEntry>()
                                   .Where(w => w.RequestId == requestId &&
                                               w.TechnicianUserId == technicianUserId &&
                                               w.StoppedAt == null)
                                   .ToListAsync(ct);

            var now = System.DateTimeOffset.UtcNow;
            foreach (var w in actives)
                w.StoppedAt = now;
        }
    }
}
