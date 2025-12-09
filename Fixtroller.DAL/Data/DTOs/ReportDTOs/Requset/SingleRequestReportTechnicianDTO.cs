using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using System;
using System.Collections.Generic;

namespace Fixtroller.DAL.Data.DTOs.Reports
{
    public class SingleRequestReportTechnicianDTO
    {
        public string TechnicianUserId { get; set; } = default!;
        public string TechnicianName { get; set; } = default!;
        public string? TechnicianCategory { get; set; }

        public DateTime AssignedAtUtc { get; set; }
        public DateTime? UnassignedAtUtc { get; set; }

        public DateTime? FirstWorkStartedAtUtc { get; set; }
        public DateTime? LastWorkStoppedAtUtc { get; set; }
        public double TotalWorkMinutes { get; set; }

        public int? ExpectedDurationHours { get; set; }
    }

    public class SingleRequestReportDTO
    {
        // بيانات الطلب الأساسية
        public int RequestId { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }

        public string ProblemTypeName { get; set; } = default!;
        public string PriorityName { get; set; } = default!;
        public string CaseTypeName { get; set; } = default!;

        // صاحب الطلب / الإنشاء
        public string OwnerFullName { get; set; } = default!;
        public string? OwnerDepartment { get; set; }
        public string? OwnerLocation { get; set; }
        public string? RequestAddress { get; set; }

        public bool IsCreatedByOwner { get; set; }
        public string CreatedByFullName { get; set; } = default!;

        // أوقات مهمة
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? FirstAssignedAtUtc { get; set; }
        public DateTime? FirstWorkStartedAtUtc { get; set; }
        public DateTime? ClosedAtUtc { get; set; }

        // SLA ومدة الإغلاق
        public double? ExpectedDurationHours { get; set; }   // SLA
        public double? ActualDurationHours { get; set; }     // مدة الإغلاق الفعلية
        public bool? IsWithinSla { get; set; }               // داخل / خارج SLA

        // الفنيين المشاركين
        public List<SingleRequestReportTechnicianDTO> Technicians { get; set; } = new();

        // الملاحظات (نستخدم DTO الموجود عندك)
        public List<MaintenanceNoteDTO> Notes { get; set; } = new();
    }
}
