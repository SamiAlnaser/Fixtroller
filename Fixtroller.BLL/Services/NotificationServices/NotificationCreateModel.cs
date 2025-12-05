using Fixtroller.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.NotificationServices
{
    public class NotificationCreateModel
    {
        public string UserId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;

        public int? MaintenanceRequestId { get; set; }

        public NotificationType Type { get; set; } = NotificationType.General;
        public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;

        public NotificationChannel Channels { get; set; } =
            NotificationChannel.InApp | NotificationChannel.Email;
    }
}
