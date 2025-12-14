using Fixtroller.DAL.Data;
using Fixtroller.DAL.Data.DTOs.PagedResultDTOs.Responses;
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

        public async Task<IReadOnlyList<ApplicationUser>> GetAsync(
            int? technicianCategoryId,
            string? search,
            CancellationToken ct = default)
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
                q = q.Where(u => u.TechnicianCategoryId == technicianCategoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(u =>
                    (u.FullName != null && u.FullName.Contains(s)) ||
                    (u.Email != null && u.Email.Contains(s)));
            }

            return await q
                .OrderBy(u => u.FullName)
                .ToListAsync(ct);
        }

        public Task<ApplicationUser?> GetByIdAsync(string userId, CancellationToken ct = default)
        {
            return _dbcontext.Users
                .AsNoTracking()
                .Include(u => u.TechnicianCategory)
                    .ThenInclude(c => c.Translations)
                .FirstOrDefaultAsync(u => u.Id == userId, ct);
        }

        public Task<bool> IsInRoleAsync(string userId, string roleName = "Technician", CancellationToken ct = default)
        {
            return _dbcontext.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .AnyAsync(roleId =>
                    _dbcontext.Roles.AsNoTracking()
                        .Any(r => r.Id == roleId && r.Name == roleName), ct);
        }

        public async Task<bool> UpdateCategoryAsync(string userId, int technicianCategoryId, CancellationToken ct = default)
        {
            // لازم Tracking لأننا سنعدّل على الكيان
            var user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null) return false;

            var exists = await _dbcontext.Tcategories
                .AnyAsync(c => c.Id == technicianCategoryId && c.Status == Status.Active, ct);
            if (!exists) return false;

            user.TechnicianCategoryId = technicianCategoryId;
            // لا حفظ هنا — UoW سيحفظ
            return true;
        }

        public async Task<bool> ClearCategoryAsync(string userId, CancellationToken ct = default)
        {
            // لازم Tracking لأننا سنعدّل على الكيان
            var user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null) return false;

            // مسح الربط مع القسم
            user.TechnicianCategoryId = null;
            // لا حفظ هنا — UoW سيحفظ
            return true;
        }

        public async Task<IReadOnlyList<ApplicationUser>> GetByCategoryAsync(int categoryId, string? search, CancellationToken ct = default)
        {
            var q = _dbcontext.Users
                .AsNoTracking()
                .Include(u => u.TechnicianCategory)
                    .ThenInclude(c => c.Translations)
                .Where(u =>
                    _dbcontext.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Select(ur => ur.RoleId)
                        .Any(roleId => _dbcontext.Roles.Any(r => r.Id == roleId && r.Name == "Technician")))
                .Where(u => u.TechnicianCategoryId == categoryId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(u =>
                    (u.FullName != null && u.FullName.Contains(s)) ||
                    (u.Email != null && u.Email.Contains(s)));
            }

            return await q.OrderBy(u => u.FullName).ToListAsync(ct);
        }

        public async Task<PagedResultDTO<ApplicationUser>> GetPagedAsync(
            string? search,
            string? status,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var nowUtc = DateTimeOffset.UtcNow;

            var q = _dbcontext.Users
                .AsNoTracking()
                .Include(u => u.TechnicianCategory)
                    .ThenInclude(c => c.Translations)
                .Where(u =>
                    _dbcontext.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Select(ur => ur.RoleId)
                        .Any(roleId => _dbcontext.Roles.Any(r => r.Id == roleId && r.Name == "Technician"))
                );

            // 🔎 Search by name
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(u => u.FullName != null && u.FullName.Contains(s));
            }

            // ✅ Filter by status: available / vacation
            if (!string.IsNullOrWhiteSpace(status))
            {
                var st = status.Trim().ToLowerInvariant();

                if (st == "vacation")
                    q = q.Where(u => u.LockoutEnd != null && u.LockoutEnd > nowUtc);

                else if (st == "available")
                    q = q.Where(u => u.LockoutEnd == null || u.LockoutEnd <= nowUtc);
            }

            var totalCount = await q.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var data = await q
                .OrderBy(u => u.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResultDTO<ApplicationUser>
            {
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                TotalCount = totalCount,
                PageSize = pageSize,
                Data = data.ToList()
            };
        }

        public async Task<(int Total, int Available, int Vacation)> GetAvailabilityCountsAsync(
    CancellationToken ct = default)
        {
            var nowUtc = DateTimeOffset.UtcNow;

            var q = _dbcontext.Users
                .AsNoTracking()
                .Where(u =>
                    _dbcontext.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Select(ur => ur.RoleId)
                        .Any(roleId => _dbcontext.Roles.Any(r => r.Id == roleId && r.Name == "Technician"))
                );

            var total = await q.CountAsync(ct);

            var vacation = await q
                .Where(u => u.LockoutEnd != null && u.LockoutEnd > nowUtc)
                .CountAsync(ct);

            var available = total - vacation;

            return (total, available, vacation);
        }
    }

}
