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
        Task<IReadOnlyList<ApplicationUser>> GetAsync(int? technicianCategoryId, string? search);
        Task<ApplicationUser?> GetByIdAsync(string userId);
        Task<bool> IsInRoleAsync(string userId, string roleName = "Technician");
        Task<bool> UpdateCategoryAsync(string userId, int technicianCategoryId);
        Task<IReadOnlyList<ApplicationUser>> GetByCategoryAsync(int categoryId, string? search);
    }
}
