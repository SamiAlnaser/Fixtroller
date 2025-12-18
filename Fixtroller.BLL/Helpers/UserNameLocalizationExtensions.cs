using Fixtroller.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Helpers
{
    public static class UserNameLocalizationExtensions
    {
        public static string GetDisplayName(this ApplicationUser? user, string language)
        {
            if (user is null) return string.Empty;

            var lang = string.IsNullOrWhiteSpace(language)
                ? "ar"
                : language.Trim().ToLowerInvariant();

            // لو اللغة إنجليزي وجاهز اسم إنجليزي
            if (lang.StartsWith("en", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(user.FullNameEn))
            {
                return user.FullNameEn!;
            }

            // باقي الحالات: عربي
            if (!string.IsNullOrWhiteSpace(user.FullNameAr))
                return user.FullNameAr;

            // fallback أخير لو في مشكلة بالأسماء
            return user.UserName ?? user.Email ?? user.Id;
        }
    }
}
