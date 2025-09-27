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
            if (asTracking)
                return await _dbcontext.Set<T>()
                      .Where(c => c.Status == Status.Active)
                      .ToListAsync();

            return await _dbcontext.Set<T>()
                      .Where(c => c.Status == Status.Active)
                      .AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(bool asTracking = false)
        {
            if (asTracking)
                return await _dbcontext.Set<T>().ToListAsync();

            return await _dbcontext.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task<T>? GetByIdAsync(int id) => await _dbcontext.Set<T>().FirstOrDefaultAsync(c => c.Id == id);


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
    }
}
