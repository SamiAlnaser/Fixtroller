using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.NotificationServices
{
    public interface IAppEmailSender
    {
        Task<bool> SendAsync(string to, string subject, string body, CancellationToken ct = default);
    }
}
