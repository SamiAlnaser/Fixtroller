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
    }

    public class MaintenanceRequestResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public Priority Priority { get; set; }
        public string CaseType { get; set; } = default!;
        public string? Address { get; set; }
        public string CreatedByUserId { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public TechnicianResponseDTO? AssignedTechnician { get; set; }

        // فقط الصور المتعددة
        public List<MaintenanceRequestImageDTO> Images { get; set; } = new();
    }
}
