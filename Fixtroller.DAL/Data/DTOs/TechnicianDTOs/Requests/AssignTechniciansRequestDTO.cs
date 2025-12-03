using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests
{
    public class AssignTechniciansRequestDTO
    {
        [Required]
        [MinLength(1)]
        public List<string> TechnicianUserIds { get; set; } = new();

        [Range(1, int.MaxValue)]
        public int? ExpectedDuration { get; set; }
    }
}
