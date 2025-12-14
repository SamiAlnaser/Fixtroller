using Fixtroller.BLL.Services.NotificationServices;

namespace Fixtroller.PL.Services.Notifications.Push
{
    public class NoopPushNotificationSender : IPushNotificationSender
    {
        public Task SendAsync(string userId, string title, string body, CancellationToken ct = default)
        {
            // حالياً لا نفعل شيء، فقط تجهيز للمستقبل
            return Task.CompletedTask;
        }
    }
}
