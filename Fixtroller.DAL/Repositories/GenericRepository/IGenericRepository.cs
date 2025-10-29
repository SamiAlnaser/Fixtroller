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
        Task AddAsync(T entity, CancellationToken ct = default);
        Task<IEnumerable<T>> GetAllAsync(bool asTracking = false, CancellationToken ct = default);
        Task<IEnumerable<T>> GetActiveAsync(bool asTracking = false, CancellationToken ct = default);
        Task<T?> GetByIdAsync(int id, bool asTracking = false, CancellationToken ct = default);
        Task UpdateAsync(T entity, CancellationToken ct = default);
        Task RemoveAsync(T entity, CancellationToken ct = default);
    }
}
