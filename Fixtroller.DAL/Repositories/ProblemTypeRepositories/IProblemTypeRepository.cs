using Fixtroller.DAL.DTOs.ProblemTypeDTOs.Responses;
using Fixtroller.DAL.Entities.ProblemTypeEntity;
using Fixtroller.DAL.Repositories.GenericRepository;

namespace Fixtroller.DAL.Repositories.ProblemTypeRepositories
{
    public interface IProblemTypeRepository : IGenericRepository<ProblemType>
    {
        Task<IEnumerable<ProblemType>> GetAllForUserAsync(
            bool asTracking = false,
            CancellationToken ct = default);

        Task<IEnumerable<ProblemType>> GetActiveForUserAsync(
            bool asTracking = false,
            CancellationToken ct = default);

        Task<ProblemType?> GetByIdForUserAsync(
            int id,
            CancellationToken ct = default);
    }
}