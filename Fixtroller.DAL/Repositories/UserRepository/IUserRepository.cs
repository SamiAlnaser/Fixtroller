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

    }
}
