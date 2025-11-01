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
    public sealed class MaintenanceRequestTechnicianRepository : IMaintenanceRequestTechnicianRepository
    {
        private readonly ApplicationDbContext _context;
        public MaintenanceRequestTechnicianRepository(ApplicationDbContext context) => _context = context;

        public IQueryable<MaintenanceRequestTechnician> Query(bool asTracking = false)
            => asTracking ? _context.Set<MaintenanceRequestTechnician>() : _context.Set<MaintenanceRequestTechnician>().AsNoTracking();

        public Task<bool> IsActiveAssignedAsync(int requestId, string technicianUserId, CancellationToken ct = default)
            => _context.Set<MaintenanceRequestTechnician>()
                  .AnyAsync(t => t.RequestId == requestId &&
                                 t.TechnicianUserId == technicianUserId &&
                                 t.UnassignedAtUtc == null, ct);

        public Task<List<string>> GetActiveTechniciansAsync(int requestId, CancellationToken ct = default)
            => _context.Set<MaintenanceRequestTechnician>()
                  .Where(t => t.RequestId == requestId && t.UnassignedAtUtc == null)
                  .Select(t => t.TechnicianUserId)
                  .Distinct()
                  .ToListAsync(ct);

        public async Task AddActiveAsync(int requestId, string technicianUserId, CancellationToken ct = default)
        {
            var exists = await IsActiveAssignedAsync(requestId, technicianUserId, ct);
            if (exists) return;

            _context.Set<MaintenanceRequestTechnician>().Add(new MaintenanceRequestTechnician
            {
                RequestId = requestId,
                TechnicianUserId = technicianUserId,
                AssignedAtUtc = System.DateTime.UtcNow,
                UnassignedAtUtc = null
            });
        }

        public async Task RemoveActiveAsync(int requestId, string technicianUserId, CancellationToken ct = default)
        {
            var active = await _context.Set<MaintenanceRequestTechnician>()
                .Where(t => t.RequestId == requestId &&
                            t.TechnicianUserId == technicianUserId &&
                            t.UnassignedAtUtc == null)
                .ToListAsync(ct);

            if (active.Count == 0) return;

            var now = System.DateTime.UtcNow;
            foreach (var t in active)
                t.UnassignedAtUtc = now;
        }

        public async Task SetActiveListAsync(int requestId, IEnumerable<string> technicianUserIds, CancellationToken ct = default)
        {
            var target = technicianUserIds.Distinct().ToHashSet();
            var current = await _context.Set<MaintenanceRequestTechnician>()
                                   .Where(t => t.RequestId == requestId && t.UnassignedAtUtc == null)
                                   .ToListAsync(ct);

            var now = System.DateTime.UtcNow;

            // mark removed as unassigned
            foreach (var c in current)
                if (!target.Contains(c.TechnicianUserId))
                    c.UnassignedAtUtc = now;

            // add new
            var currentSet = current.Select(c => c.TechnicianUserId).ToHashSet();
            foreach (var toAdd in target.Except(currentSet))
                _context.Set<MaintenanceRequestTechnician>().Add(new MaintenanceRequestTechnician
                {
                    RequestId = requestId,
                    TechnicianUserId = toAdd,
                    AssignedAtUtc = now
                });
        }
    }
}
