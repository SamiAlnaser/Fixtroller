using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.NotificationServices
{
    public interface INotificationMessageBuilder
    {
        (string Title, string Body) Build(
            string titleKey, object[]? titleArgs,
            string bodyKey, object[]? bodyArgs,
            string language);
    }
}
