using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.ProblemType;
using Fixtroller.DAL.Entities.TechnicianCategory;
using Fixtroller.DAL.Repositories.GenericRepository;
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
        public async Task<IEnumerable<TechnicianCategory>> GetActiveForUserAsync(bool asTracking)
        {
            if (asTracking)
                return await _dbcontext.Tcategories.Include(p => p.Translations)
                      .Where(c => c.Status == Status.Active)
                      .ToListAsync();

            return await _dbcontext.Tcategories.Include(p => p.Translations)
                      .Where(c => c.Status == Status.Active)
                      .AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<TechnicianCategory>> GetAllForUserAsync(bool asTracking)
        {
            if (asTracking)
                return await _dbcontext.Tcategories.Include(p => p.Translations).ToListAsync();

            return await _dbcontext.Tcategories.Include(p => p.Translations).AsNoTracking().ToListAsync();
        }

        public async Task<TechnicianCategory>? GetByIdForUserAsync(int id) => await _dbcontext.Tcategories.Include(p => p.Translations).FirstOrDefaultAsync(c => c.Id == id);

    }
}

