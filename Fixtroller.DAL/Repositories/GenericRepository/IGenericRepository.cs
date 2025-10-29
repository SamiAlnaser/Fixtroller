using Fixtroller.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.GenericRepository
{
    public interface IGenericRepository<T> where T : BaseModel
    {
        Task AddAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync(bool asTracking = false);
        Task<IEnumerable<T>> GetActiveAsync(bool asTracking = false);
        Task<T?> GetByIdAsync(int id, bool asTracking = false);
        Task UpdateAsync(T entity);
        Task RemoveAsync(T entity);
    }
}
