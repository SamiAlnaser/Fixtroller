using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.NotificationRepositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<List<Notification>> GetForUserAsync(
            string userId,
            bool onlyUnread,
            CancellationToken ct = default);

        Task<Notification?> GetForUserByIdAsync(
            int id,
            string userId,
            bool asTracking = true,
            CancellationToken ct = default);

        Task<List<Notification>> GetUnreadForUserAsync(
            string userId,
            CancellationToken ct = default);

        Task<List<Notification>> GetForUserPageAsync(
            string userId,
            bool onlyUnread,
            int take,
            int? lastId,
            CancellationToken ct = default);
    }
}
