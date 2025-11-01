using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.MaintenanceRequestEntity
{
    public class WorkTimeEntry : BaseModel
    {
        public int RequestId { get; set; }
        public MaintenanceRequest Request { get; set; } = default!;

        public string TechnicianUserId { get; set; } = default!; // الفني المُعيّن وقت البدء
        public DateTimeOffset StartedAt { get; set; }            // UTC
        public DateTimeOffset? StoppedAt { get; set; }        
    }
}
