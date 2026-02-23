using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.MaintenanceRequestRepositories
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

        public async Task AddActiveAsync(int requestId, string technicianUserId, int? expectedDuration, CancellationToken ct = default)
        {
            var exists = await IsActiveAssignedAsync(requestId, technicianUserId, ct);
            if (exists) return;

            _context.Set<MaintenanceRequestTechnician>().Add(new MaintenanceRequestTechnician
            {
                RequestId = requestId,
                TechnicianUserId = technicianUserId,
                AssignedAtUtc = System.DateTime.UtcNow,
                ExpectedDuration = expectedDuration,
                UnassignedAtUtc = null,
                IsLead = false, // يتحدد لاحقاً
                TechnicianStatus = TechnicianTaskStatus.Assigned
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

        public async Task SetActiveListAsync(int requestId, IEnumerable<string> technicianUserIds, int? expectedDuration, CancellationToken ct = default)
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
                    AssignedAtUtc = now,
                    ExpectedDuration = expectedDuration,
                    UnassignedAtUtc = null,
                    IsLead = false,
                    TechnicianStatus = TechnicianTaskStatus.Assigned
                });
        }

        public Task<bool> IsLeadAsync(int requestId, string technicianUserId, CancellationToken ct = default)
        {
            return _context.Set<MaintenanceRequestTechnician>()
                .AnyAsync(t =>
                    t.RequestId == requestId &&
                    t.TechnicianUserId == technicianUserId &&
                    t.UnassignedAtUtc == null &&
                    t.IsLead,
                    ct);
        }

        public Task<List<MaintenanceRequestTechnician>> GetActiveTechniciansWithStatusAsync(
            int requestId,
            CancellationToken ct = default)
        {
            return _context.Set<MaintenanceRequestTechnician>()
                .Where(t => t.RequestId == requestId && t.UnassignedAtUtc == null)
                .ToListAsync(ct);
        }

        public async Task UpdateTechnicianStatusAsync(
            int requestId,
            string technicianUserId,
            TechnicianTaskStatus status,
            CancellationToken ct = default)
        {
            var tech = await _context.Set<MaintenanceRequestTechnician>()
                .FirstOrDefaultAsync(t =>
                    t.RequestId == requestId &&
                    t.TechnicianUserId == technicianUserId &&
                    t.UnassignedAtUtc == null,
                    ct);

            if (tech is null) return;

            tech.TechnicianStatus = status;
        }

        public async Task SetLeadAsync(
            int requestId,
            string technicianUserId,
            CancellationToken ct = default)
        {
            var techs = await _context.Set<MaintenanceRequestTechnician>()
                .Where(t => t.RequestId == requestId && t.UnassignedAtUtc == null)
                .ToListAsync(ct);

            foreach (var t in techs)
                t.IsLead = string.Equals(t.TechnicianUserId, technicianUserId, StringComparison.Ordinal);
        }


        public async Task SetTaskGroupAsync(
    int requestId,
    IEnumerable<string> technicianUserIds,
    string taskGroupKey,
    string? leadTechnicianUserId,
    CancellationToken ct = default)
        {
            var techIds = technicianUserIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (!techIds.Any())
                return;

            var techs = await _context.Set<MaintenanceRequestTechnician>()
                .Where(t =>
                    t.RequestId == requestId &&
                    t.UnassignedAtUtc == null &&
                    techIds.Contains(t.TechnicianUserId))
                .ToListAsync(ct);

            foreach (var t in techs)
            {
                t.TaskGroupKey = taskGroupKey;
                t.IsLead = leadTechnicianUserId != null &&
                           string.Equals(t.TechnicianUserId, leadTechnicianUserId, StringComparison.Ordinal);
            }
        }

    }
}
