using Fixtroller.DAL.Data.DTOs.AnnouncementDTOs.Responses;
using Fixtroller.DAL.Entities.Announcements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Mapping
{
    public static class AnnouncementMapper
    {
        public static AnnouncementListItemDTO ToListItem(
            Announcement a,
            string creatorName,
            Func<string, string> urlBuilder)
        {
            var shortContent = a.Content.Length > 150
                ? a.Content.Substring(0, 150) + "..."
                : a.Content;

            return new AnnouncementListItemDTO
            {
                Id = a.Id,
                Title = a.Title,
                ShortContent = shortContent,
                LinkUrl = a.LinkUrl,
                Audience = a.Audience.ToString(),
                CreatedAt = a.CreatedAt,
                CreatedByName = creatorName
            };
        }

        public static AnnouncementDetailsDTO ToDetails(
            Announcement a,
            string creatorName,
            Func<string, string> urlBuilder)
        {
            return new AnnouncementDetailsDTO
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                LinkUrl = a.LinkUrl,
                Audience = a.Audience.ToString(),
                CreatedAt = a.CreatedAt,
                CreatedByName = creatorName,
                Images = a.Images
                        .Select(i => new AnnouncementImageDTO
                        {
                            Id = i.Id,
                            Url = urlBuilder(i.FileName)
                        })
                        .ToList()
            };
        }
    }
}
