using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses
{
    public class MaintenanceRequestListMineDTO
    {
        public int Id { get; set; }                   
        public string Title { get; set; } = default!; 
        public string CaseType { get; set; } = default!;
        public string Priority { get; set; } = default!;
        public string? ProblemTypeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; } 
    }

    public class MaintenanceRequestListAllDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string CaseType { get; set; } = default!;
        public string Priority { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string? ProblemTypeName { get; set; }
        public string? AssignedTechnicianUserId { get; set; }
        public string? AssignedTechnicianName { get; set; }
    }

}
