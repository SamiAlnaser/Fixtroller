using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests
{
    public class MaintenanceRequestScenarioRequestDTO : MaintenanceRequestRequestDTO
    {
        [Required]
        public string OwnerUserId { get; set; } = string.Empty;

        [Required]
        public CaseType CaseType { get; set; }
    }
}
