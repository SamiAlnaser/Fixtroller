using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses
{
    public class AssignedTechnicianDTO
    {
        public string UserId { get; set; } = default!;
        public DateTime AssignedAtUtc { get; set; }
    }
}
