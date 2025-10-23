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
        Modified = 9  // معدل
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
        public string Description { get; set; }
        public CaseType CaseType { get; set; } = CaseType.Submitted;
        public ICollection<MaintenanceRequestImage> Images { get; set; } = new List<MaintenanceRequestImage>();
        

        public string Address { get; set; }
        public Priority Priority { get; set; }

        public string CreatedByUserId { get; set; }            
        public ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ProblemTypeId { get; set; }
        public ProblemType ProblemType { get; set; }

        public string? AssignedTechnicianUserId { get; set; }
        public ApplicationUser AssignedTechnician { get; set; }
        public DateTime? AssignedAtUtc { get; set; }
    }
}
