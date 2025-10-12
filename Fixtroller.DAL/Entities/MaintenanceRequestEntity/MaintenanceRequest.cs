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
        Processed = 3,  // تمت المعالجة
        Completed = 4, // مكتمل
        ResourcesNeeded = 5, // يحتاج الى موارد او فني اخر
        Cancelled = 6,  // ملغى
        Reopened = 7, // تم اعادة فتح الطلب
        Modified = 8  // معدل
    }
    public enum Priority
    {
        Low =1,
        Medium =2,
        High =3
    }
    public class MaintenanceRequest : BaseModel
    {
        public int RequestNumber { get; set; }
        public string Title {  get; set; }
        public string Description { get; set; }
        public CaseType CaseType { get; set; } = CaseType.Submitted;
        public string? MainImage { get; set; }
        public string Address { get; set; }
        public Priority Priority { get; set; }

        public string CreatedByUserId { get; set; }            
        public ApplicationUser CreatedByUser { get; set; }

        public int ProblemTypeId { get; set; }
        public ProblemType ProblemType { get; set; }
    }
}
