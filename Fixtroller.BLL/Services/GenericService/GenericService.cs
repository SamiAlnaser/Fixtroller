
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.GenericRepository;
using Fixtroller.DAL.UnitOfWork;
using Mapster;

namespace Fixtroller.BLL.Services.GenericService
{
    public class GenericService<TRequest, TResponse, TEntity> : IGenericService<TRequest, TResponse, TEntity>
        where TEntity : BaseModel
    {
        private readonly IGenericRepository<TEntity> _repository;
        private readonly IUnitOfWork _uow;

        public GenericService(IGenericRepository<TEntity> repository, IUnitOfWork uow)
        {
            _repository = repository;
            _uow = uow;
        }

        public async Task<int> AddAsync(TRequest dto)
        {
            var entity = dto.Adapt<TEntity>();
            await _repository.AddAsync(entity);
            await _uow.SaveAndCommitAsync();
            return entity.Id; // EF يعبّي بعد الحفظ
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
            var entity = await _repository.GetByIdAsync(id, asTracking: true);
            if (entity == null) return 0;

            await _repository.RemoveAsync(entity);
            await _uow.SaveAndCommitAsync();
            return id;
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id, asTracking: true);
            if (entity == null) return false;

            entity.Status = entity.Status == Status.Active ? Status.In_active : Status.Active;
            await _repository.UpdateAsync(entity);
            await _uow.SaveAndCommitAsync();
            return true;
        }

        public async Task<int> UpdateAsync(int id, TRequest dto)
        {
            var entity = await _repository.GetByIdAsync(id, asTracking: true);
            if (entity == null) return 0;

            dto.Adapt(entity);
            await _repository.UpdateAsync(entity);
            await _uow.SaveAndCommitAsync();
            return id;
        }
    }
}
