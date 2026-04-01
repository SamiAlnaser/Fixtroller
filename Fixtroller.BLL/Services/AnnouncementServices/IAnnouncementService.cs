using Fixtroller.DAL.Data.DTOs.AnnouncementDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.AnnouncementDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.PagedResultDTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.AnnouncementServices
{
    public interface IAnnouncementService
    {

        Task<int> CreateAsync(
            AnnouncementCreateRequestDTO dto,
            string creatorUserId,
            string creatorRole,
            string language,
            CancellationToken ct = default);

        Task<int> UpdateAsync(
            int id,
            AnnouncementUpdateRequestDTO dto,
            string userId,
            string userRole,
            string language,
            CancellationToken ct = default);

        Task<bool> DeleteAsync(
            int id,
            string userId,
            string userRole,
            CancellationToken ct = default);

        Task<PagedResultDTO<AnnouncementListItemDTO>> GetForUserAsync(
            string userId,
            string userRole,
            string language,
            string? search,
            bool unreadOnly,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

        Task<AnnouncementDetailsDTO?> GetByIdForUserAsync(
            int id,
            string userId,
            string userRole,
            string language,
            CancellationToken ct = default);

        Task<bool> MarkAsReadAsync(
            int announcementId,
            string userId,
            string userRole,
            CancellationToken ct = default);

        Task<int> MarkAllAsReadAsync(
            string userId,
            string userRole,
            CancellationToken ct = default);
    }
}
