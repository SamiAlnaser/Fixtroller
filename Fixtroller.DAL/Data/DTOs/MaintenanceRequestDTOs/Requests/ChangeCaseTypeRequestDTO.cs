using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests
{
    public class ChangeCaseTypeRequestDTO
    {
        public CaseType NewCaseType { get; set; }
        public string? NoteText { get; set; }      // مطلوب لـ Reopened/ResourcesNeeded
        public NoteType? NoteType { get; set; }
        public Priority? Priority { get; set; }
    }
}
