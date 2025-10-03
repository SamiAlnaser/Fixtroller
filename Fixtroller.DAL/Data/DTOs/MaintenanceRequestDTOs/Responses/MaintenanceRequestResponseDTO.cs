using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses
{
    public class MaintenanceRequestResponseDTO
    {
        public int Id { get; set; }
        public int RequestNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
        public CaseType CaseType { get; set; }
        public string Address { get; set; }

        //ProblemTypeName
        [JsonIgnore]
        public string MainImage { get; set; }

        public string MainImageUrl => $"https://localhost:7127/Images/{MainImage}";
    }
}
