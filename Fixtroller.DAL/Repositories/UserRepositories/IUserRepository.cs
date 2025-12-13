using Fixtroller.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.UserRepository
{
    public interface IUserRepository
    {
        Task<List<ApplicationUser>> GetAllAsync(CancellationToken ct = default);

        Task<ApplicationUser?> GetByIdAsync(string userId, CancellationToken ct = default);

        Task<bool> ChangeUserRoleAsync(string userId, string roleName, CancellationToken ct = default);

        Task<List<ApplicationUser>> GetByRoleAsync(string roleName, CancellationToken ct = default);

        Task<bool> VacationUserAsync(string userId, int days, CancellationToken ct = default);

        Task<bool> UnVacationUserAsync(string userId, CancellationToken ct = default);

        Task<bool> IsVacationAsync(string userId, CancellationToken ct = default);

        // ✅ جديد لإنشاء المستخدم (Admin Create User)
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);

        Task<bool> RoleExistsAsync(string roleName, CancellationToken ct = default);

        Task<bool> CreateUserAsync(ApplicationUser user, string password, CancellationToken ct = default);

        Task<bool> AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken ct = default);

        Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct = default);
        Task<bool> UpdateAsync(ApplicationUser user, CancellationToken ct = default);

    }
}

