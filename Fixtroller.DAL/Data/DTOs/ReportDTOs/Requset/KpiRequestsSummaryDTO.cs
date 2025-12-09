using System;
using System.Collections.Generic;

namespace Fixtroller.DAL.Data.DTOs.Reports
{
    public class KpiRequestsSummaryDTO
    {
        public int TotalRequests { get; set; }

        public int NewRequests { get; set; }          // = إجمالي الطلبات في الفترة (حسب التعريف الحالي)
        public int ClosedRequests { get; set; }       // Completed + Cancelled
        public int OpenRequests { get; set; }         // غير مكتملة / غير ملغاة
        public int OverdueRequests { get; set; }      // متأخرة عن SLA
        public int RemainingRequests { get; set; }    // المتبقية (مفتوحة)

        // النِسَب (0 - 100)
        public double? CompletionRate { get; set; }   // نسبة الإنجاز
        public double? OverdueRate { get; set; }      // نسبة التأخير
        public double? SlaComplianceRate { get; set; } // نسبة الالتزام بالـ SLA (من الطلبات المغلقة ذات SLA)

        // متوسطات
        public double? AverageClosureHours { get; set; } // متوسط زمن الإغلاق (ساعة)
    }

    public class KpiTopProblemTypeDTO
    {
        public int ProblemTypeId { get; set; }
        public string ProblemTypeName { get; set; } = default!;
        public int Count { get; set; }
    }

    public class KpiTopDepartmentDTO
    {
        public string DepartmentName { get; set; } = default!;
        public int Count { get; set; }
    }

    public class KpiRequestsReportDTO
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }

        public int? ProblemTypeId { get; set; }
        public string? ProblemTypeName { get; set; }

        public KpiRequestsSummaryDTO Summary { get; set; } = new();
        public List<KpiTopProblemTypeDTO> TopProblemTypes { get; set; } = new();
        public List<KpiTopDepartmentDTO> TopDepartments { get; set; } = new();
    }
}
