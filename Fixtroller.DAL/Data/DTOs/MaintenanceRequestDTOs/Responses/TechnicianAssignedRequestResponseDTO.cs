using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses
{
    public class TechnicianAssignedRequestResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }

        public string CaseType { get; set; }   // Submitted / InProgress / Completed / Canceled
        public string Priority { get; set; }   // Low / Medium / High

        public int ProblemTypeId { get; set; }
        public string ProblemTypeName { get; set; }

        public string MainImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AssignedAtUtc { get; set; }
    }
}
