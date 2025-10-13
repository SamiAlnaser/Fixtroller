using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.UserRepository.TechnicianRepositorirs
{
    public class TechnicianRepository : ITechnicianRepository
    {
        private readonly ApplicationDbContext _dbcontext;

        public TechnicianRepository(ApplicationDbContext context) => _dbcontext = context;

        public async Task<IReadOnlyList<ApplicationUser>> GetAsync(int? technicianCategoryId, string? search)
        {
            var q = _dbcontext.Users
                .AsNoTracking()
                .Include(u => u.TechnicianCategory)
                    .ThenInclude(c => c.Translations)
              
                .Where(u =>
                    _dbcontext.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Select(ur => ur.RoleId)
                        .Any(roleId =>
                            _dbcontext.Roles.Any(r => r.Id == roleId && r.Name == "Technician")
                        )
                );

            if (technicianCategoryId.HasValue)
            {
                q = q.Where(u => u.TechnicianCategoryId == technicianCategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(u =>
                    (u.FullName != null && u.FullName.Contains(s)) ||
                    (u.Email != null && u.Email.Contains(s))
                );
            }

            return await q
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetByIdAsync(string userId)
        {
            return await _dbcontext.Users
                .AsNoTracking()
                .Include(u => u.TechnicianCategory)
                    .ThenInclude(c => c.Translations)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<bool> IsInRoleAsync(string userId, string roleName = "Technician")
        {
            return await _dbcontext.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .AnyAsync(roleId =>
                    _dbcontext.Roles
                        .AsNoTracking()
                        .Any(r => r.Id == roleId && r.Name == roleName));
        }

        public async Task<bool> UpdateCategoryAsync(string userId, int technicianCategoryId)
        {
            var user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return false;

            var exists = await _dbcontext.Tcategories.AnyAsync(c => c.Id == technicianCategoryId && c.Status == Status.Active);
            if (!exists) return false;

            user.TechnicianCategoryId = technicianCategoryId;
            await _dbcontext.SaveChangesAsync();
            return true;
        }
        public async Task<IReadOnlyList<ApplicationUser>> GetByCategoryAsync(int categoryId, string? search)
        {
            var q = _dbcontext.Users
                .AsNoTracking()
                .Include(u => u.TechnicianCategory)
                    .ThenInclude(c => c.Translations)
                // فقط من يملكون دور Technician
                .Where(u =>
                    _dbcontext.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Select(ur => ur.RoleId)
                        .Any(roleId => _dbcontext.Roles.Any(r => r.Id == roleId && r.Name == "Technician"))
                )
                .Where(u => u.TechnicianCategoryId == categoryId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(u =>
                    (u.FullName != null && u.FullName.Contains(s)) ||
                    (u.Email != null && u.Email.Contains(s)));
            }

            return await q.OrderBy(u => u.FullName).ToListAsync();
        }
    }

    }
