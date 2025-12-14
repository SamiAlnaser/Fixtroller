using Fixtroller.DAL.Data.DTOs.NotificationDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.NotificationServices
{
    public interface INotificationService
    {
        Task<int> CreateAsync(
            NotificationCreateModel model,
            CancellationToken ct = default);

        Task<IReadOnlyList<NotificationListItemDTO>> GetForUserAsync(
            string userId, bool onlyUnread, string language = "ar", CancellationToken ct = default);

        Task MarkAsReadAsync(int id, string userId, CancellationToken ct = default);

        Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);
        Task<NotificationLoadMoreResponseDTO<NotificationListItemDTO>> GetForUserPageAsync(
            string userId,
            bool onlyUnread,
            int take,
            int? lastId,
            string language = "ar",
            CancellationToken ct = default);
    }
}
