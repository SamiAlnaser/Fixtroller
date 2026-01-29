using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.Announcements
{

    public enum AnnouncementAudience
    {
        TechniciansOnly = 1,
        EmployeesAndTechnicians = 2
    }

    public class AnnouncementImage : BaseModel
    {
        public int AnnouncementId { get; set; }
        public Announcement Announcement { get; set; } = default!;

        public string FileName { get; set; } = default!;
    }

    public class Announcement : BaseModel
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }

        public AnnouncementAudience Audience { get; set; }

        public string CreatedByUserId { get; set; } = default!;
        public ApplicationUser CreatedByUser { get; set; } = default!;

        public ICollection<AnnouncementImage> Images { get; set; }
            = new List<AnnouncementImage>();
    }

}
