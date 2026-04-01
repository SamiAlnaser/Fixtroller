using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.AnnouncementDTOs.Responses
{
    public class AnnouncementListItemDTO
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string ShortContent { get; set; } = string.Empty;

        public string? LinkUrl { get; set; }

        public string Audience { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string CreatedByName { get; set; } = string.Empty;
        public bool IsRead { get; set; }

    }
}
