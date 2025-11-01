using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests
{
    public class AssignTechniciansRequestDTO
    {
        public List<string> TechnicianUserIds { get; set; } = new();
    }
}
