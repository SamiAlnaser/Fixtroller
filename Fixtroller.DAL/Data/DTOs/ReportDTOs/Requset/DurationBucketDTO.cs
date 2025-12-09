using System;
using System.Collections.Generic;

namespace Fixtroller.DAL.Data.DTOs.Reports
{
    public class DurationBucketDTO
    {
        public string BucketKey { get; set; } = default!;   // lt12h, h12to72, gt72h
        public string BucketName { get; set; } = default!;  // "أقل من 12 ساعة" ...
        public int Count { get; set; }
        public double Percentage { get; set; }              // من إجمالي المكتملة %
    }

    public class ProblemTypeDurationMetricsDTO
    {
        public int ProblemTypeId { get; set; }
        public string ProblemTypeName { get; set; } = default!;
        public int CompletedCount { get; set; }

        // متوسط زمن الإغلاق (ساعة)
        public double? AverageClosureHours { get; set; }

        // نسبة الطلبات المتأخرة (من الطلبات ذات SLA)
        public double? OverdueRate { get; set; }
    }

    public class DurationByProblemTypeReportDTO
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }

        public int TotalCompleted { get; set; }

        // تقسيم حسب مدة الإغلاق
        public List<DurationBucketDTO> Buckets { get; set; } = new();

        // لكل نوع مشكلة
        public List<ProblemTypeDurationMetricsDTO> ProblemTypes { get; set; } = new();
    }
}
