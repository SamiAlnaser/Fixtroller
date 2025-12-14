using Fixtroller.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.NotificationDTOs
{
    public class NotificationListItemDTO
    {
        public int Id { get; set; }

        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;

        // ✅ لازم يكونوا دايمًا موجودين
        public string TitleKey { get; set; } = default!;
        public string BodyKey { get; set; } = default!;
        public string? TitleArgsJson { get; set; }
        public string? BodyArgsJson { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public NotificationType Type { get; set; }
        public NotificationSeverity Severity { get; set; }

        public int? MaintenanceRequestId { get; set; }

        public string? RequestNumberLabel =>
            MaintenanceRequestId.HasValue ? $"#{MaintenanceRequestId.Value}" : null;
    }
}
