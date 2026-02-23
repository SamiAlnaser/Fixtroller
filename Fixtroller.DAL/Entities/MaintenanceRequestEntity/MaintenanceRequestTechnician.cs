using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.MaintenanceRequestEntity
{
    public enum TechnicianTaskStatus
    {
        Assigned = 1,            // متعيّن على الطلب
        Processing = 2,          // شغال حالياً (نستخدمها لاحقاً مع StartWork إن حبيت)
        WaitingManagerReview = 3,// خلّص شغله وبستنى مراجعة المدير
        ResourcesNeeded = 4,     // يحتاج موارد / مساعدة
        Completed = 5            // أنهى مهمته تماماً
    }
    public class MaintenanceRequestTechnician : BaseModel
    {
        public int RequestId { get; set; }
        public MaintenanceRequest Request { get; set; } = default!;

        public string TechnicianUserId { get; set; } = default!;
        public int? ExpectedDuration { get; set; }

        public DateTime AssignedAtUtc { get; set; }
        public DateTime? UnassignedAtUtc { get; set; } // null = تعيين نشط
        public string? TaskGroupKey { get; set; }
        public bool IsActive => UnassignedAtUtc == null;

        public bool IsLead { get; set; }

        public TechnicianTaskStatus TechnicianStatus { get; set; }
            = TechnicianTaskStatus.Assigned;
    }
}
