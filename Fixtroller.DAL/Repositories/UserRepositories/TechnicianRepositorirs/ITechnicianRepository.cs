using Fixtroller.DAL.Data.DTOs.PagedResultDTOs.Responses;
using Fixtroller.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.UserRepository.TechnicianRepositorirs
{
    public interface ITechnicianRepository
    {
        Task<IReadOnlyList<ApplicationUser>> GetAsync(
            int? technicianCategoryId,
            string? search,
            CancellationToken ct = default);

        Task<ApplicationUser?> GetByIdAsync(
            string userId,
            CancellationToken ct = default);

        Task<bool> IsInRoleAsync(
            string userId,
            string roleName = "Technician",
            CancellationToken ct = default);

        Task<bool> UpdateCategoryAsync(
            string userId,
            int technicianCategoryId,
            CancellationToken ct = default);
        Task<bool> ClearCategoryAsync(
    string userId,
    CancellationToken ct = default);

        Task<IReadOnlyList<ApplicationUser>> GetByCategoryAsync(
            int categoryId,
            string? search,
            CancellationToken ct = default);
        Task<PagedResultDTO<ApplicationUser>> GetPagedAsync(
            string? search,
            string? status,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default);
        Task<(int Total, int Available, int Vacation)> GetAvailabilityCountsAsync(
    CancellationToken ct = default);
    }
}
