using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests
{
    public class AddImagesRequestDTO
    {
        public List<IFormFile> Images { get; set; } = new();
        // اختياري: نخلي أول صورة Primary
        public bool MakePrimaryFirst { get; set; } = true;
    }
}
