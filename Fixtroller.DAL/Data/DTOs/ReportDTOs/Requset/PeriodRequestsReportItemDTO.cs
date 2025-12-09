using System;
using System.Collections.Generic;

namespace Fixtroller.DAL.Data.DTOs.Reports
{
    public class PeriodRequestsReportItemDTO
    {
        public int RequestId { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public string ProblemTypeName { get; set; } = default!;
        public string CaseTypeName { get; set; } = default!;

        public string? MainTechnicianName { get; set; }
        public DateTime? ClosedAtUtc { get; set; }

        // هل داخل SLA أو لا (null لو مافي SLA)
        public bool? IsWithinSla { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class PeriodRequestsReportSummaryDTO
    {
        public int TotalRequests { get; set; }
        public int CompletedCount { get; set; }
        public int OpenCount { get; set; }
        public int CancelledCount { get; set; }
        public int OverdueCount { get; set; }
    }

    public class PeriodRequestsReportDTO
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }

        public int? ProblemTypeId { get; set; }
        public string? ProblemTypeName { get; set; }

        public PeriodRequestsReportSummaryDTO Summary { get; set; } = new();
        public List<PeriodRequestsReportItemDTO> Items { get; set; } = new();
    }
}
