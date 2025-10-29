using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses
{
    public class MaintenanceNoteDTO
    {
        public int Id { get; set; }
        public string Text { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string Author { get; set; } = default!;
        public string CreatedByUserId { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
