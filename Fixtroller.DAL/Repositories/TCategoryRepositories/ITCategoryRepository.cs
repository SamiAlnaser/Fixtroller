using Fixtroller.DAL.Entities.ProblemType;
using Fixtroller.DAL.Entities.TechnicianCategory;
using Fixtroller.DAL.Repositories.GenericRepository;

namespace Fixtroller.DAL.Repositories.TCategoryRepositories
{
    public interface ITCategoryRepository : IGenericRepository<TechnicianCategory>
    {
        Task<IEnumerable<TechnicianCategory>> GetAllForUserAsync(bool asTracking = false);
        Task<IEnumerable<TechnicianCategory>> GetActiveForUserAsync(bool asTracking = false);
        Task<TechnicianCategory>? GetByIdForUserAsync(int id);
    }
}