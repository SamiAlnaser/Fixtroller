using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.GenericRepositories
{

    public class GenericRepository<T> : IGenericRepository<T> where T : BaseModel
    {
        private readonly ApplicationDbContext _dbcontext;

        public GenericRepository(ApplicationDbContext context)
        {
            _dbcontext = context;
        }

        public async Task AddAsync(T entity, CancellationToken ct = default)
        {
            await _dbcontext.Set<T>().AddAsync(entity, ct); // لا Save هنا
        }

        public async Task<IEnumerable<T>> GetActiveAsync(bool asTracking = false, CancellationToken ct = default)
        {
            IQueryable<T> q = _dbcontext.Set<T>().Where(c => c.Status == Status.Active);
            q = ApplyTranslationsInclude(q);
            if (!asTracking) q = q.AsNoTracking();
            return await q.ToListAsync(ct);
        }

        public async Task<IEnumerable<T>> GetAllAsync(bool asTracking = false, CancellationToken ct = default)
        {
            IQueryable<T> q = _dbcontext.Set<T>();
            q = ApplyTranslationsInclude(q);
            if (!asTracking) q = q.AsNoTracking();
            return await q.ToListAsync(ct);
        }

        public Task<T?> GetByIdAsync(int id, bool asTracking = false, CancellationToken ct = default)
        {
            IQueryable<T> q = _dbcontext.Set<T>();
            q = ApplyTranslationsInclude(q);
            if (!asTracking) q = q.AsNoTracking();
            return q.FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public Task RemoveAsync(T entity, CancellationToken ct = default)
        {
            _dbcontext.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            _dbcontext.Set<T>().Update(entity);
            return Task.CompletedTask;
        }

        private IQueryable<T> ApplyTranslationsInclude(IQueryable<T> query)
        {
            var entityType = _dbcontext.Model.FindEntityType(typeof(T));
            if (entityType is null) return query;

            foreach (var nav in entityType.GetNavigations())
            {
                if (nav.IsCollection && nav.Name.EndsWith("Translations", StringComparison.OrdinalIgnoreCase))
                    query = query.Include(nav.Name);
            }
            return query;
        }
    }
}

