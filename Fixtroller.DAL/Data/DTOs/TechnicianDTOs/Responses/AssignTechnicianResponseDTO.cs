using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses
{
    public class AssignTechnicianResponseDTO
    {
        public int MaintenanceRequestId { get; set; }
        public TechnicianResponseDTO Technician { get; set; }
        public DateTime AssignedAtUtc { get; set; }
    }
}
