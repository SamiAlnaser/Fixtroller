using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests
{
    public class RemoveStaffImagesRequestDTO
    {
        [Required]
        [MinLength(1)]
        public List<int> ImageIds { get; set; } = new();
    }
}
