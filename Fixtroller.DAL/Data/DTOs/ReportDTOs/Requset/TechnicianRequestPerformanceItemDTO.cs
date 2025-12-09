using System;
using System.Collections.Generic;

namespace Fixtroller.DAL.Data.DTOs.Reports
{
    public class TechnicianRequestPerformanceItemDTO
    {
        public int RequestId { get; set; }
        public string Title { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; }

        public string ProblemTypeName { get; set; } = default!;
        public string CaseTypeName { get; set; } = default!;

        public DateTime AssignedAtUtc { get; set; }
        public DateTime? FirstWorkStartedAtUtc { get; set; }
        public DateTime? ClosedAtUtc { get; set; }

        // SLA
        public int? ExpectedDurationHours { get; set; }
        public bool HasSla => ExpectedDurationHours.HasValue;
        public bool? IsOverdue { get; set; }  // null لو ما في SLA

        // المدد
        public double? ClosureHours { get; set; }      // من الإنشاء للإغلاق
        public double? StartDelayHours { get; set; }   // من التعيين لأول بدء عمل للفني
    }

    public class TechnicianPerformanceSummaryDTO
    {
        public int AssignedCount { get; set; }
        public int CompletedCount { get; set; }

        public int OverdueCount { get; set; }          // طلبات متأخرة عن SLA
        public double? OverdueRate { get; set; }       // نسبة المتأخرة من الطلبات ذات SLA

        public double? AverageClosureHours { get; set; }
        public double? AverageStartDelayHours { get; set; }
    }

    public class TechnicianPerformanceReportDTO
    {
        // بيانات الفني
        public string TechnicianUserId { get; set; } = default!;
        public string TechnicianName { get; set; } = default!;
        public string? TechnicianCategoryName { get; set; }

        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }

        public TechnicianPerformanceSummaryDTO Summary { get; set; } = new();
        public List<TechnicianRequestPerformanceItemDTO> Items { get; set; } = new();
    }
}
