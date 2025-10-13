using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests
{
    public class UpdateTechnicianCategoryRequestDTO
    {
        public string TechnicianUserId { get; set; }
        public int TechnicianCategoryId { get; set; }
    }
}
