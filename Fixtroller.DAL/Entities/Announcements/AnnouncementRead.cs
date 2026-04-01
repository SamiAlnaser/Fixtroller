using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.Announcements
{
    public class AnnouncementRead : BaseModel
    {
        public int AnnouncementId { get; set; }
        public Announcement Announcement { get; set; } = default!;

        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    }
}
