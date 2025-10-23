using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests
{
    public class MaintenanceRequestUpdateDTO
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public Priority? Priority { get; set; }
        public int? ProblemTypeId { get; set; }

        public List<IFormFile>? NewImages { get; set; }
        public List<int>? RemoveImageIds { get; set; }
    }
}
