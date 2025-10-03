using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.ProblemTypeEntity;
using Fixtroller.DAL.Repositories.GenericRepository;
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

        public async Task<IEnumerable<ProblemType>> GetActiveForUserAsync(bool asTracking = false)
        {
            if (asTracking)
                return await _dbcontext.PTypes.Include(p => p.Translations)
                      .Where(c => c.Status == Status.Active)
                      .ToListAsync();

            return await _dbcontext.Set<ProblemType>().Include(p => p.Translations)
                      .Where(c => c.Status == Status.Active)
                      .AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<ProblemType>> GetAllForUserAsync(bool asTracking = false)
        {
            if (asTracking)
                return await _dbcontext.PTypes.Include(p => p.Translations).ToListAsync();

            return await _dbcontext.Set<ProblemType>().Include(p => p.Translations).AsNoTracking().ToListAsync();
        }

        public async Task<ProblemType>? GetByIdForUserAsync(int id) => await _dbcontext.Set<ProblemType>().Include(p => p.Translations).FirstOrDefaultAsync(c => c.Id == id);

    }
}
