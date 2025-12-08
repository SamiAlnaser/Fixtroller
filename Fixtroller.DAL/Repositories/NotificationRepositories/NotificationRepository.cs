using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.NotificationRepositories
{
    public sealed class NotificationRepository
            : GenericRepository<Notification>, INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        private IQueryable<Notification> QueryBase(bool asTracking = false)
        {
            var set = _context.Set<Notification>().AsQueryable();
            if (!asTracking) set = set.AsNoTracking();
            return set;
        }

        public Task<List<Notification>> GetForUserAsync(
            string userId,
            bool onlyUnread,
            CancellationToken ct = default)
        {
            var query = QueryBase(false)
                .Where(n => n.UserId == userId);

            if (onlyUnread)
                query = query.Where(n => !n.IsRead);

            return query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(ct);
        }

        public Task<Notification?> GetForUserByIdAsync(
            int id,
            string userId,
            bool asTracking = true,
            CancellationToken ct = default)
        {
            return QueryBase(asTracking)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);
        }

        public Task<List<Notification>> GetUnreadForUserAsync(
            string userId,
            CancellationToken ct = default)
        {
            return QueryBase(true)
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(ct);
        }
    }
}
