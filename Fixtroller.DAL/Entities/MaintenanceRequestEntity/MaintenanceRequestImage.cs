using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.MaintenanceRequestEntity
{
    public class MaintenanceRequestImage : BaseModel
    {
        public int MaintenanceRequestId { get; set; }
        public MaintenanceRequest MaintenanceRequest { get; set; } = default!;

        public string FileName { get; set; } = default!;
        public bool IsPrimary { get; set; } //  نعلّم صورة أساسية
    }
}
