using Fixtroller.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.GenericService
{
    public interface IGenericService<TRequest, TResponse, TEntity>
       where TEntity : BaseModel
    {
        Task<IEnumerable<TResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<TResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<TResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> AddAsync(TRequest dto, CancellationToken ct = default);
        Task<int> UpdateAsync(int id, TRequest dto, CancellationToken ct = default);
        Task<int> RemoveAsync(int id, CancellationToken ct = default);
        Task<bool> ToggleStatusAsync(int id, CancellationToken ct = default);
    }
}
