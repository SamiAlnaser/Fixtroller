using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Entities.ProblemTypeEntity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests
{
    public class MaintenanceRequestRequestDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<IFormFile>? Images { get; set; } // صور متعددة
        public string Address { get; set; }
        public Priority Priority { get; set; }
        public int ProblemTypeId { get; set; }
    }
}
