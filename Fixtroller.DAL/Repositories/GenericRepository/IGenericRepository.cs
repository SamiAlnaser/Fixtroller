using Fixtroller.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.GenericRepository
{
    public interface IGenericRepository <T> where T : BaseModel
    {
        Task<int> AddAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync(bool asTracking = false);
        Task<IEnumerable<T>> GetActiveAsync(bool asTracking = false);
        Task<T>? GetByIdAsync(int id);
        Task<int> UpdateAsync(T entity);
        Task<int> RemoveAsync(T entity);
    }
}
