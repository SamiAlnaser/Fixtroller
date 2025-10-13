using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.GenericRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.MaintenanceRequestepository
{
    public class MaintenanceRequestRepository : GenericRepository<MaintenanceRequest>, IMaintenanceRequestRepository
    {
        private readonly ApplicationDbContext _context;
        public MaintenanceRequestRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<MaintenanceRequest> Query(bool asTracking = false, Func<IQueryable<MaintenanceRequest>, IQueryable<MaintenanceRequest>>? include = null, Expression<Func<MaintenanceRequest, bool>>? predicate = null)
        {
            IQueryable<MaintenanceRequest> q = _context.MaintenanceRequests;
            if (!asTracking) q = q.AsNoTracking();
            if (include is not null) q = include(q);
            if (predicate is not null) q = q.Where(predicate);
            return q;
        }
        public async Task<MaintenanceRequest?> GetForAssignmentAsync(int id)
        {
            return await _context.MaintenanceRequests
                .Include(r => r.AssignedTechnician)
                    .ThenInclude(t => t.TechnicianCategory)
                        .ThenInclude(c => c.Translations)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        public IQueryable<MaintenanceRequest> QueryAssignedTo(string technicianUserId, bool asTracking = false)
        {
            var q = asTracking ? _context.MaintenanceRequests
                               : _context.MaintenanceRequests.AsNoTracking();

            return q
                .Include(r => r.ProblemType)
                    .ThenInclude(pt => pt.Translations) // ← مهم
                .Where(r => r.AssignedTechnicianUserId == technicianUserId)
                .OrderByDescending(r => r.AssignedAtUtc)
                .ThenByDescending(r => r.CreatedAt);
        }
        public async Task<MaintenanceRequest?> GetForUpdateAsync(int id)
        {
            return await _context.MaintenanceRequests
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public Task CommitAsync() => _context.SaveChangesAsync();
    }
}
