using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests
{
    public class MaintenanceRequestUpdateDTO
    {
        [StringLength(150)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        public Priority? Priority { get; set; }

        [Range(1, int.MaxValue)]
        public int? ProblemTypeId { get; set; }

        public List<IFormFile>? NewImages { get; set; }

        public List<int>? RemoveImageIds { get; set; }
    }
}
