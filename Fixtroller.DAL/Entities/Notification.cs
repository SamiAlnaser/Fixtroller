using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities
{
    public enum NotificationType
    {
        General = 0,
        RequestAssigned = 1,
        RequestStatusChanged = 2,
        RequestCompleted = 3,
        ExtraInfoRequired = 4,
        ScheduleReminder = 5
    }

    public enum NotificationSeverity
    {
        Info = 0,
        Success = 1,
        Warning = 2,
        Error = 3
    }

    [Flags]
    public enum NotificationChannel
    {
        None = 0,
        InApp = 1,
        Email = 2,
        MobilePush = 4
    }



    public class Notification : BaseModel
    {
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        public string TitleKey { get; set; } = default!;
        public string BodyKey { get; set; } = default!;
        public string? TitleArgsJson { get; set; }
        public string? BodyArgsJson { get; set; }

        public int? MaintenanceRequestId { get; set; }
        public NotificationType Type { get; set; }
        public NotificationSeverity Severity { get; set; }
        public NotificationChannel Channels { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool EmailSent { get; set; } = false;
    }
}

