using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses
{
    public class RequestTechnicianItemDTO
    {
        public string UserId { get; set; } = default!;
        public string? FullName { get; set; }
        public DateTime AssignedAtUtc { get; set; }
        public int? ExpectedDuration { get; set; }

        public bool HasActiveWork { get; set; }

        public bool IsLead { get; set; }

        public string? TaskGroupKey { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TechnicianTaskStatus? TechnicianStatus { get; set; }
    }

    public class RequestTechniciansViewDTO
    {
        public int RequestId { get; set; }
        public TechnicianAssignmentMode TechnicianAssignmentMode { get; set; }
        public List<RequestTechnicianItemDTO> Technicians { get; set; } = new();
    }
}
