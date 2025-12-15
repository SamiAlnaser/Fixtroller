using System;
using System.Collections.Generic;

namespace Fixtroller.DAL.Data.DTOs.Reports.Responses
{
    public class TechnicianCategoryTechItemDTO
    {
        public string TechnicianUserId { get; set; } = default!;
        public string TechnicianName { get; set; } = default!;

        public int AssignedCount { get; set; }
        public int CompletedCount { get; set; }
        public int OverdueCount { get; set; }

        // متوسط زمن الإغلاق (ساعة) لطلبات هذا الفني
        public double? AverageClosureHours { get; set; }
    }

    public class TechnicianCategoryPerformanceDTO
    {
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = default!; // مثلاً "غير مصنّف" لو null

        public int TechniciansCount { get; set; }

        public int TotalAssigned { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalOverdue { get; set; }

        public double? CompletionRate { get; set; }          // نسبة الإنجاز
        public double? OverdueRate { get; set; }             // نسبة المتأخرة
        public double? AverageClosureHours { get; set; }     // متوسط زمن الإغلاق للطلبات
        public double? AverageRequestsPerTechnician { get; set; } // ضغط العمل

        public List<TechnicianCategoryTechItemDTO> Technicians { get; set; } = new();
    }

    public class TechnicianCategoriesPerformanceReportDTO
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }

        public List<TechnicianCategoryPerformanceDTO> Categories { get; set; } = new();
    }
}
