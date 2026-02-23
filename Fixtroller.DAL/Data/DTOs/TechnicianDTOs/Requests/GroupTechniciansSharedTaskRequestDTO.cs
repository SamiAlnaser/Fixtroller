using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests
{
    public class GroupTechniciansSharedTaskRequestDTO
    {
        [Required]
        [MinLength(2, ErrorMessage = "Group_TwoTechniciansRequired")]
        public List<string> TechnicianUserIds { get; set; } = new();

        [Required]
        public string LeadTechnicianUserId { get; set; } = default!;
    }
}
