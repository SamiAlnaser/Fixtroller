using System;
using System.Collections.Generic;

namespace Fixtroller.DAL.Data.DTOs.Reports
{
    public class MaintenanceDepartmentCategoryStatDTO
    {
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = default!;

        public int TechniciansCount { get; set; }
        public int RequestsCount { get; set; }
    }

    public class MaintenanceDepartmentReportDTO
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }

        // الأرقام العامة (نستخدم نفس KpiRequestsSummaryDTO)
        public KpiRequestsSummaryDTO Summary { get; set; } = new();

        // عدد الفنيين الكلي
        public int TotalTechnicians { get; set; }

        // توزيع الفنيين و الطلبات على الـ Categories
        public List<MaintenanceDepartmentCategoryStatDTO> Categories { get; set; } = new();

        // أكثر أنواع المشاكل تكرارًا (Top 3 من نفس KpiTopProblemTypeDTO)
        public List<KpiTopProblemTypeDTO> TopProblemTypes { get; set; } = new();

        // أكثر الـ Categories من حيث عدد الطلبات (Top 3)
        public List<MaintenanceDepartmentCategoryStatDTO> TopCategoriesByRequests { get; set; } = new();
    }
}
