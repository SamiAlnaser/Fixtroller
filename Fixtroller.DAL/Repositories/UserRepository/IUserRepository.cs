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
        Task<List<ApplicationUser>> GetAllAsync();

        Task<ApplicationUser> GetByIdAsync(string userId);
        Task<bool> ChangeUserRoleAsync(string userId, string roleName);
        Task<bool> BlockUserAsync(string userId, int days);
        Task<bool> UnBlockUserAsync(string userId);
        Task<bool> IsBlockedAsync(string userId);

    }
}
