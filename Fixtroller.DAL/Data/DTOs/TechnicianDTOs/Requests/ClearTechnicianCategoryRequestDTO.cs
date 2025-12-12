using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests
{
    public class ClearTechnicianCategoryRequestDTO
    {
        [Required]
        public string TechnicianUserId { get; set; } = default!;
    }
}
