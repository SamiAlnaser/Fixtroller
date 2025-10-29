
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

        public async Task<int> AddAsync(TRequest dto, CancellationToken ct = default)
        {
            var entity = dto.Adapt<TEntity>();
            await _repository.AddAsync(entity);                 
            await _uow.SaveAndCommitAsync(ct);
            return entity.Id; // EF يعبّي بعد الحفظ
        }

        public async Task<IEnumerable<TResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var entities = await _repository.GetActiveAsync(asTracking: false); 
            return entities.Adapt<IEnumerable<TResponse>>();
        }

        public async Task<IEnumerable<TResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var entities = await _repository.GetAllAsync(asTracking: false);  
            return entities.Adapt<IEnumerable<TResponse>>();
        }

        public async Task<TResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id);                    
            return entity is null ? default : entity.Adapt<TResponse>();
        }

        public async Task<int> RemoveAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, asTracking: true); 
            if (entity is null) return 0;

            await _repository.RemoveAsync(entity);                              
            await _uow.SaveAndCommitAsync(ct);
            return id;
        }

        public async Task<bool> ToggleStatusAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, asTracking: true);  
            if (entity is null) return false;

            entity.Status = entity.Status == Status.Active ? Status.In_active : Status.Active;
            await _repository.UpdateAsync(entity);                              
            await _uow.SaveAndCommitAsync(ct);
            return true;
        }

        public async Task<int> UpdateAsync(int id, TRequest dto, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, asTracking: true);  
            if (entity is null) return 0;

            dto.Adapt(entity);
            await _repository.UpdateAsync(entity);                              
            await _uow.SaveAndCommitAsync(ct);
            return id;
        }
    }
}
