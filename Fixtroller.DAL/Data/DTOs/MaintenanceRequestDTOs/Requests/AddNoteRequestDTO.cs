using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests
{
    public class AddNoteRequestDTO
    {
        public string Text { get; set; } = default!;
        public NoteType? Type { get; set; } 
    }
}
