using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Entities.TechnicianCategoryEntity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Fixtroller.DAL.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } // username رح يتغير عشان موضوع اللغات و رح يتغير تبعياته في السيرفر و التوكين و غيره مع  
        public string Location { get; set; }
        public string? Department { get; set; }
        public int? TechnicianCategoryId { get; set; }
        public string? ProfileImagePath { get; set; }

        public TechnicianCategory TechnicianCategory { get; set; }
        // الطلبات التي "باسمه" كصاحب الطلب
        public ICollection<MaintenanceRequest> OwnedRequests { get; set; } = new List<MaintenanceRequest>();

        public ICollection<MaintenanceRequest> SubmittedRequests { get; set; } = new List<MaintenanceRequest>();
    }

}
