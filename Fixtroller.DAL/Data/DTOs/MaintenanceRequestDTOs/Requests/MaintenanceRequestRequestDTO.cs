using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Entities.ProblemTypeEntity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests
{
    public class MaintenanceRequestRequestDTO
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        public List<IFormFile>? Images { get; set; }

        [Required]
        [StringLength(250)]
        public string Address { get; set; }

        [Required]
        public Priority Priority { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int ProblemTypeId { get; set; }
    }
}
