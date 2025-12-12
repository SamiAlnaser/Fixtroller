using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.MaintenanceRequestRepositories
{
    public class MaintenanceRequestRepository : GenericRepository<MaintenanceRequest>, IMaintenanceRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceRequestRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<MaintenanceRequest> Query(
            bool asTracking = false,
            Func<IQueryable<MaintenanceRequest>, IQueryable<MaintenanceRequest>>? include = null,
            Expression<Func<MaintenanceRequest, bool>>? predicate = null)
        {
            IQueryable<MaintenanceRequest> q = _context.MaintenanceRequests;
            if (!asTracking) q = q.AsNoTracking();
            if (include is not null) q = include(q);
            if (predicate is not null) q = q.Where(predicate);
            return q;
        }

        public IQueryable<MaintenanceRequest> QueryAssignedTo(string technicianUserId, bool asTracking = false)
        {
            var q = asTracking ? _context.MaintenanceRequests
                               : _context.MaintenanceRequests.AsNoTracking();

            return q
                .Include(r => r.ProblemType)
                    .ThenInclude(pt => pt.Translations)
                .Include(r => r.Technicians.Where(t => t.UnassignedAtUtc == null)) // حمّل التعيينات النشطة
                .Where(r => r.Technicians.Any(t =>
                    t.UnassignedAtUtc == null &&
                    t.TechnicianUserId == technicianUserId))
                .OrderByDescending(r => r.Technicians
                    .Where(t => t.UnassignedAtUtc == null && t.TechnicianUserId == technicianUserId)
                    .Select(t => t.AssignedAtUtc)
                    .Max()) // أحدث تاريخ تعيين لهذا الفني على هذا الطلب
                .ThenByDescending(r => r.CreatedAt);
        }

        public Task<MaintenanceRequest?> GetForAssignmentAsync(int id, CancellationToken ct = default)
        {
            return _context.MaintenanceRequests
                .Include(r => r.Technicians.Where(t => t.UnassignedAtUtc == null))
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public Task<MaintenanceRequest?> GetForUpdateAsync(int id, CancellationToken ct = default)
        {
            return _context.MaintenanceRequests
                .Include(r => r.Technicians.Where(t => t.UnassignedAtUtc == null))
                .Include(r => r.Images)
                .Include(r => r.Notes)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }
    }
}
