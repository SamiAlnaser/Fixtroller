
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.GenericRepository;
using Mapster;

namespace Fixtroller.BLL.Services.GenericService
{
     public class GenericService<TRequest, TResponse, TEntity> : IGenericService<TRequest, TResponse, TEntity> 
        where TEntity : BaseModel
    {
        private readonly IGenericRepository<TEntity> _repository;

        public GenericService(IGenericRepository<TEntity> repository)
        {
            _repository = repository;
        }

        public async Task<int> AddAsync(TRequest dto)
        {
            var entity = dto.Adapt<TEntity>();
            return await _repository.AddAsync(entity);
        }

        public async Task<IEnumerable<TResponse>> GetActiveAsync()
        {
            var entity = await _repository.GetActiveAsync();
            return entity.Adapt<IEnumerable<TResponse>>();
        }

        public async Task<IEnumerable<TResponse>> GetAllAsync()
        {
            var entity = await _repository.GetAllAsync();
            return entity.Adapt<IEnumerable<TResponse>>();
        }

        public async Task<TResponse>? GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? default : entity.Adapt<TResponse>();
        }

        public async Task<int> RemoveAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? 0 : await _repository.RemoveAsync(entity);
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return false;

            entity.Status = entity.Status == Status.Active ? Status.In_active : Status.Active;
            await _repository.UpdateAsync(entity);
            return true;
        }

        public async Task<int> UpdateAsync(int id, TRequest dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return 0;

            dto.Adapt(entity);
            return await _repository.UpdateAsync(entity);
        }
    }
}
