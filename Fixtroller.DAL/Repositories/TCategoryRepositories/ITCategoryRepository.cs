using Fixtroller.DAL.Entities.TechnicianCategoryEntity;
using Fixtroller.DAL.Repositories.GenericRepository;

namespace Fixtroller.DAL.Repositories.TCategoryRepositories
{
    public interface ITCategoryRepository : IGenericRepository<TechnicianCategory>
    {
        Task<IEnumerable<TechnicianCategory>> GetAllForUserAsync(
            bool asTracking = false,
            CancellationToken ct = default);

        Task<IEnumerable<TechnicianCategory>> GetActiveForUserAsync(
            bool asTracking = false,
            CancellationToken ct = default);

        Task<TechnicianCategory?> GetByIdForUserAsync(
            int id,
            CancellationToken ct = default);
    }
}