using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.MaintenanceRequestEntity
{
    public class MaintenanceRequestTechnician : BaseModel
    {
        public int RequestId { get; set; }
        public MaintenanceRequest Request { get; set; } = default!;

        public string TechnicianUserId { get; set; } = default!;
        public DateTime AssignedAtUtc { get; set; }
        public DateTime? UnassignedAtUtc { get; set; } // null = تعيين نشط

        public bool IsActive => UnassignedAtUtc == null;
    }
}
