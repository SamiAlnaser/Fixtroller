using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.TechnicianCategoryEntity;
using Fixtroller.DAL.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.TCategoryRepositories
{
    public class TCategorRepository : GenericRepository<TechnicianCategory>, ITCategoryRepository
    {
        private readonly ApplicationDbContext _dbcontext;

        public TCategorRepository(ApplicationDbContext context) : base(context)
        {
            _dbcontext = context;
        }
        public async Task<IEnumerable<TechnicianCategory>> GetAllForUserAsync(
            bool? isActive = null,
            bool asTracking = false,
            CancellationToken ct = default)
        {
            IQueryable<TechnicianCategory> q = _dbcontext.Tcategories
                .Include(p => p.Translations);

            if (!asTracking)
                q = q.AsNoTracking();

            if (isActive.HasValue)
            {
                var wantedStatus = isActive.Value ? Status.Active : Status.In_active;
                q = q.Where(t => t.Status == wantedStatus);
            }

            return await q.ToListAsync(ct);
        }

        public async Task<IEnumerable<TechnicianCategory>> GetActiveForUserAsync(
            bool asTracking = false,
            CancellationToken ct = default)
        {
            IQueryable<TechnicianCategory> q = _dbcontext.Tcategories
                .Include(p => p.Translations)
                .Where(c => c.Status == Status.Active);

            if (!asTracking)
                q = q.AsNoTracking();

            return await q.ToListAsync(ct);
        }

        public Task<TechnicianCategory?> GetByIdForUserAsync(
            int id,
            CancellationToken ct = default)
        {
            return _dbcontext.Tcategories
                .AsNoTracking()
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }
    }
}

