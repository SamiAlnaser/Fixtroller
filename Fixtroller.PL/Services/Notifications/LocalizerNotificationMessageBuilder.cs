using Fixtroller.BLL.Services.NotificationServices;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Fixtroller.PL.Services.Notifications
{
    public sealed class LocalizerNotificationMessageBuilder : INotificationMessageBuilder
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        public LocalizerNotificationMessageBuilder(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

        public (string Title, string Body) Build(
            string titleKey, object[]? titleArgs,
            string bodyKey, object[]? bodyArgs,
            string language)
        {
            language = string.IsNullOrWhiteSpace(language) ? "ar" : language;
            var culture = language.StartsWith("en", StringComparison.OrdinalIgnoreCase)
                ? new CultureInfo("en")
                : new CultureInfo("ar");

            var oldUi = CultureInfo.CurrentUICulture;
            var old = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentUICulture = culture;
                CultureInfo.CurrentCulture = culture;

                var titleTemplate = _localizer[titleKey].Value;
                var bodyTemplate = _localizer[bodyKey].Value;

                var title = (titleArgs == null || titleArgs.Length == 0)
                    ? titleTemplate
                    : string.Format(titleTemplate, titleArgs);

                var body = (bodyArgs == null || bodyArgs.Length == 0)
                    ? bodyTemplate
                    : string.Format(bodyTemplate, bodyArgs);

                return (title, body);
            }
            finally
            {
                CultureInfo.CurrentUICulture = oldUi;
                CultureInfo.CurrentCulture = old;
            }
        }
    }
}
