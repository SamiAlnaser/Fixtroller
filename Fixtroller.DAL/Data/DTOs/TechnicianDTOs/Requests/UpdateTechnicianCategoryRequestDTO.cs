using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests
{
    public class UpdateTechnicianCategoryRequestDTO
    {
        [Required]
        public string TechnicianUserId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int TechnicianCategoryId { get; set; }
    }
}
