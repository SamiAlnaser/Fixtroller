using Fixtroller.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace Fixtroller.DAL.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<ApplicationUser>> GetAllAsync(
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            return await _userManager.Users
                .ToListAsync(ct);
        }

        public async Task<ApplicationUser?> GetByIdAsync(
            string userId,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<List<ApplicationUser>> GetByRoleAsync(
    string roleName,
    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return new List<ApplicationUser>();

            // UserManager ما بدعم CancellationToken هنا، فنطنّش ct
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

            // نرجّع List<ApplicationUser> عادية
            return usersInRole.ToList();
        }

        public async Task<bool> ChangeUserRoleAsync(
            string userId,
            string roleName,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return false;

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded) return false;
            }

            var addResult = await _userManager.AddToRoleAsync(user, roleName);
            return addResult.Succeeded;
        }

        public async Task<bool> VacationUserAsync(
            string userId,
            int days,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return false;

            user.LockoutEnd = DateTimeOffset.UtcNow.AddDays(days);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UnVacationUserAsync(
            string userId,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return false;

            user.LockoutEnd = null;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> IsVacationAsync(
            string userId,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return false;

            return user.LockoutEnd.HasValue &&
                   user.LockoutEnd > DateTimeOffset.UtcNow;
        }


    }
}
