using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
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
        public ICollection<MaintenanceRequest> SubmittedRequests { get; set; } = new List<MaintenanceRequest>();
    }

}
