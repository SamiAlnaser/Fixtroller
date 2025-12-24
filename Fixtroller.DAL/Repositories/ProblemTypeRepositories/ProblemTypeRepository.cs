using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.ProblemTypeEntity;
using Fixtroller.DAL.Repositories.GenericRepositories;
using Fixtroller.DAL.Repositories.ProblemTypeRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.ProblemTypeRepositories
{
    public class ProblemTypeRepository : GenericRepository<ProblemType>, IProblemTypeRepository
    {
        private readonly ApplicationDbContext _dbcontext;

        public ProblemTypeRepository(ApplicationDbContext context) : base(context)
        {
            _dbcontext = context;
        }

        public async Task<IEnumerable<ProblemType>> GetActiveForUserAsync(
            bool asTracking = false,
            CancellationToken ct = default)
        {
            IQueryable<ProblemType> q = _dbcontext.PTypes
                .Include(p => p.Translations)
                .Where(c => c.Status == Status.Active);

            if (!asTracking) q = q.AsNoTracking();

            return await q.ToListAsync(ct);
        }

        public async Task<IEnumerable<ProblemType>> GetAllForUserAsync(
            bool? isActive = null,
            bool asTracking = false,
            CancellationToken ct = default)
        {
            IQueryable<ProblemType> q = _dbcontext.PTypes
                .Include(p => p.Translations);

            if (!asTracking)
                q = q.AsNoTracking();

            if (isActive.HasValue)
            {
                // true → Active
                // false → In_active
                var wantedStatus = isActive.Value ? Status.Active : Status.In_active;
                q = q.Where(p => p.Status == wantedStatus);
            }

            return await q.ToListAsync(ct);
        }


        public Task<ProblemType?> GetByIdForUserAsync(
            int id,
            CancellationToken ct = default)
        {
            return _dbcontext.PTypes
                .AsNoTracking()
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }
    }
}
