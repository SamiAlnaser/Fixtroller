using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses
{
    public sealed class TechnicianListItemDTO
    {
        public string TechnicianUserId { get; set; } = "";
        public string TechnicianName { get; set; } = "";
        public string? TechnicianCategory { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int AssignedCount { get; set; }        
        public int CompletedCount { get; set; }        
        public int AvgCompletionMinutes { get; set; }     
    }
}
