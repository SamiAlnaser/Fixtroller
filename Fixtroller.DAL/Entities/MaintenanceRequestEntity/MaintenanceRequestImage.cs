using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.MaintenanceRequestEntity
{

    public enum MaintenanceRequestImageSource
    {
        RequestCreation = 1,   // تمت اضافتها من مالك الطلب
        StaffAttachment = 2    // من AddImagesAsync (مدير/فني/أدمن)
    }
    public class MaintenanceRequestImage : BaseModel
    {
        public int MaintenanceRequestId { get; set; }
        public MaintenanceRequest MaintenanceRequest { get; set; } = default!;

        public string FileName { get; set; } = default!;
        public bool IsPrimary { get; set; } //  نعلّم صورة أساسية

        public MaintenanceRequestImageSource Source { get; set; }
             = MaintenanceRequestImageSource.RequestCreation;

    }
}
