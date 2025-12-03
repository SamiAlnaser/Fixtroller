using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests
{
    public class AddNoteRequestDTO
    {
        [Required]
        [StringLength(1000)]
        public string Text { get; set; } = default!;
        public NoteType? Type { get; set; } 
    }
}
