using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fixtroller.DAL.Entities.ProblemTypeEntity;

namespace Fixtroller.DAL.Entities.MaintenanceRequestEntity
{
    public enum CaseType
    {
        Submitted = 1, // تم تقديم الطلب
        Processing = 2, // قيد المعالجة
        ManagerReview = 3,
        Processed = 4,  // تمت المعالجة
        Completed = 5, // مكتمل
        ResourcesNeeded = 6, // يحتاج الى موارد او فني اخر
        Cancelled = 7,  // ملغى
        Reopened = 8, // تم اعادة فتح الطلب
        Modified = 9,  // معدل
        NotProcessed = 10 // لم تتم معالجته (قرار من قسم الصيانة/الإدارة)
    }
    public enum TechnicianAssignmentMode
    {
        Single = 1,             // فني واحد مسؤول
        TeamShared = 2,         // فريق يشترك في نفس المهمة (Lead واحد فقط يغيّر الحالة)
        ParallelIndependent = 3 // فريق، كل فني له مهمة مستقلة وحالته الخاصة
    }
    public enum Priority
    {
        Low =1,
        Medium =2,
        High =3
    }
    public class MaintenanceRequest : BaseModel
    {
        public string Title {  get; set; }
        public string? Description { get; set; }
        public CaseType CaseType { get; set; } = CaseType.Submitted;
        public ICollection<MaintenanceRequestImage> Images { get; set; } = new List<MaintenanceRequestImage>();
        public ICollection<MaintenanceNote> Notes { get; set; } = new List<MaintenanceNote>();


        public string? Address { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public Priority Priority { get; set; }


        // صاحب الطلب (الموظف اللي الطلب باسمه)
        public string OwnerUserId { get; set; }
        public ApplicationUser OwnerUser { get; set; }

        // الشخص اللي أنشأ الطلب فعلياً في النظام (موظف/فني/مدير)
        public string CreatedByUserId { get; set; }            
        public ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
        public int ProblemTypeId { get; set; }
        public ProblemType ProblemType { get; set; }
        public TechnicianAssignmentMode TechnicianAssignmentMode { get; set; }
            = TechnicianAssignmentMode.Single;

        public ICollection<MaintenanceRequestTechnician> Technicians { get; set; }
            = new List<MaintenanceRequestTechnician>();
    }
}
