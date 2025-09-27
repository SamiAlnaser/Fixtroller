using Fixtroller.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.GenericService
{
    public interface IGenericService<TRequest,TResponse,TEntity> where TEntity : BaseModel
    {
        Task<IEnumerable<TResponse>> GetAllAsync();
        Task<IEnumerable<TResponse>> GetActiveAsync();
        Task<TResponse>? GetByIdAsync(int id);
        Task<int> AddAsync(TRequest dto);
        Task<int> UpdateAsync(int id, TRequest dto);
        Task<int> RemoveAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
    }
}
