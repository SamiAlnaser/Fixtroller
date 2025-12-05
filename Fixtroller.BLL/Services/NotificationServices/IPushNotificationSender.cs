using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.NotificationServices
{
    public interface IPushNotificationSender
    {
        Task SendAsync(string userId, string title, string body, CancellationToken ct = default);
    }
}
