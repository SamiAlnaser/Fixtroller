using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.MaintenanceRequestEntity
{
    public enum NoteType
    {
        General = 1,        // ملاحظة عادية
        ReopenReason = 2,   // سبب إعادة الفتح (إلزامي بحالة Reopened)
        HelpRequest = 3,    // عند طلب المساعدة (إلزامي بحالة ResourcesNeeded)
        ManagerReviewReason = 4, // سبب تحويل الطلب لمراجعة المدير
        NotProcessedReason = 5    // سبب عدم معالجة الطلب (إلزامي لحالة NotProcessed)
    }

    public enum NoteAuthor
    {
        Owner = 1,
        Technician = 2,
        Manager = 3,
        Admin = 4
    }

    public class MaintenanceNote : BaseModel
    {
        public int MaintenanceRequestId { get; set; }
        public MaintenanceRequest MaintenanceRequest { get; set; } = default!;

        public string Text { get; set; } = default!;
        public NoteType Type { get; set; } = NoteType.General;
        public NoteAuthor Author { get; set; } = NoteAuthor.Owner;

        public string CreatedByUserId { get; set; } = default!;
        public ApplicationUser CreatedByUser { get; set; } = default!;
    }
}
