using Fixtroller.DAL.Entities.TechnicianCategoryEntity;
using Fixtroller.DAL.Repositories.GenericRepositories;

namespace Fixtroller.DAL.Repositories.TCategoryRepositories
{
    public interface ITCategoryRepository : IGenericRepository<TechnicianCategory>
    {
        Task<IEnumerable<TechnicianCategory>> GetAllForUserAsync(
            bool? isActive = null,
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