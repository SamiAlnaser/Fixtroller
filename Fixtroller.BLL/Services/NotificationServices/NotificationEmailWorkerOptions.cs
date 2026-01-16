using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.NotificationServices
{
    public sealed class NotificationEmailWorkerOptions
    {
        // كل كم ثانية يفحص الإشعارات
        public int PollIntervalSeconds { get; set; } = 20;

        // أقصى عدد إشعارات في كل دورة
        public int BatchSize { get; set; } = 50;
    }
}
