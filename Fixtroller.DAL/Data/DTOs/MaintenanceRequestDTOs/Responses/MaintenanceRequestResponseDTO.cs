using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses
{
    public class MaintenanceRequestImageDTO
    {
        public int Id { get; set; }
        public string Url { get; set; } = default!;
        public bool IsPrimary { get; set; }

        public bool IsStaffAttachment { get; set; }
    }

    public class MaintenanceRequestResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public Priority Priority { get; set; }
        public string PriorityName { get; set; } = "";
        public string CaseType { get; set; } = default!;

        public int ProblemTypeId { get; set; }
        public string? ProblemTypeName { get; set; }
        public string? Address { get; set; }

        public string OwnerUserId { get; set; } = default!;
        public string CreatedByUserId { get; set; } = default!;
        public bool IsCreatedByOwner { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
        public List<AssignedTechnicianDTO> AssignedTechnicians { get; set; } = new();

        // فقط الصور المتعددة
        public List<MaintenanceRequestImageDTO> Images { get; set; } = new();
        public List<MaintenanceNoteDTO> Notes { get; set; } = new();


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? CurrentTechnicianActiveSeconds { get; set; }
    }
}
