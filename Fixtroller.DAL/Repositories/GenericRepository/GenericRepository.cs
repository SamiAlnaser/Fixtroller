using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.GenericRepository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseModel
    {
        private readonly ApplicationDbContext _dbcontext;

        public GenericRepository(ApplicationDbContext context)
        {
            _dbcontext = context;
        }

        public async Task<int> AddAsync(T entity)
        {
            await _dbcontext.Set<T>().AddAsync(entity);
            await _dbcontext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<IEnumerable<T>> GetActiveAsync(bool asTracking = false)
        {
            IQueryable<T> q = _dbcontext.Set<T>().Where(c => c.Status == Status.Active);
            q = ApplyTranslationsInclude(q);

            if (!asTracking) q = q.AsNoTracking();
            return await q.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(bool asTracking = false)
        {
            IQueryable<T> q = _dbcontext.Set<T>();
            q = ApplyTranslationsInclude(q);

            if (!asTracking) q = q.AsNoTracking();
            return await q.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            IQueryable<T> q = _dbcontext.Set<T>();
            q = ApplyTranslationsInclude(q);

            return await q.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<int> RemoveAsync(T entity)
        {
            _dbcontext.Set<T>().Remove(entity);
            return await _dbcontext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(T entity)
        {
            _dbcontext.Set<T>().Update(entity);
            await _dbcontext.SaveChangesAsync();
            return entity.Id;
        }

        // === helper ===
        private IQueryable<T> ApplyTranslationsInclude(IQueryable<T> query)
        {
            var entityType = _dbcontext.Model.FindEntityType(typeof(T));
            if (entityType is null) return query;

            
            foreach (var nav in entityType.GetNavigations())
            {
                if (nav.IsCollection &&
                    nav.Name.EndsWith("Translations", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Include(nav.Name);
                }
            }
            return query;
        }
    }
}
