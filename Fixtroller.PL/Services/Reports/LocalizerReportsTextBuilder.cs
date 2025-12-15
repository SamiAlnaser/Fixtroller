using Fixtroller.BLL.Reports;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Fixtroller.PL.Services.Reports
{
    public sealed class LocalizerReportsTextBuilder : IReportsTextBuilder
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        public LocalizerReportsTextBuilder(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

        public string Get(string key, string language, params object[] args)
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

                var template = _localizer[key].Value ?? key;

                if (args == null || args.Length == 0)
                    return template;

                return string.Format(template, args);
            }
            finally
            {
                CultureInfo.CurrentUICulture = oldUi;
                CultureInfo.CurrentCulture = old;
            }
        }
    }
}
