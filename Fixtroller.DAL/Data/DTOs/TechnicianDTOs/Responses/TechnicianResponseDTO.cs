using Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses
{
    public class TechnicianResponseDTO
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public TCategoryUserResponseDTO TechnicianCategory { get; set; }
    }
}
