using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Fixtroller.BLL.Reports
{
    public static class ReportBranding
    {
        // شعار الجامعة في الهيدر
        private static readonly byte[] _logoBytes = LoadLogoBytes();

        // شعار تطبيق Fixtroller في التذييل
        private static readonly byte[] _appLogoBytes = LoadAppLogoBytes();

        public static byte[] LogoBytes => _logoBytes;

        private static byte[] LoadLogoBytes()
        {
            try
            {
                var baseDir = AppContext.BaseDirectory;

                var candidates = new[]
                {
                    // بعد الـ publish
                    Path.Combine(baseDir, "wwwroot", "Images", "logo", "logo1.webp"),

                    // وقت التطوير من تحت bin/Debug/netX
                    Path.GetFullPath(
                        Path.Combine(baseDir, "..", "..", "..",
                            "wwwroot", "Images", "logo", "logo1.webp"))
                };

                foreach (var path in candidates)
                {
                    if (File.Exists(path))
                        return File.ReadAllBytes(path);
                }
            }
            catch
            {
            }

            return Array.Empty<byte>();
        }

        private static byte[] LoadAppLogoBytes()
        {
            try
            {
                var baseDir = AppContext.BaseDirectory;

                var candidates = new[]
                {
                    // بعد الـ publish
                    Path.Combine(baseDir, "wwwroot", "Images", "logo", "fixtroller-logo.jpg"),

                    // وقت التطوير من تحت bin/Debug/netX
                    Path.GetFullPath(
                        Path.Combine(baseDir, "..", "..", "..",
                            "wwwroot", "Images", "logo", "fixtroller-logo.jpg"))
                };

                foreach (var path in candidates)
                {
                    if (File.Exists(path))
                        return File.ReadAllBytes(path);
                }
            }
            catch
            {
            }

            return Array.Empty<byte>();
        }

        /// <summary>
        /// هيدر موحد: شعار بالأعلى، تحته العنوان ثم سطر فرعي (مثلاً الفترة)، ثم خط فاصل.
        /// </summary>
        public static void RenderHeader(
            IContainer container,
            string title,
            string? subtitle)
        {
            container.Column(col =>
            {
                // الشعار
                if (_logoBytes.Length > 0)
                {
                    col.Item()
                       .AlignCenter()
                       .Width(260)
                       .Height(55)
                       .Image(_logoBytes);
                }

                // العنوان
                col.Item()
                   .PaddingTop(10)
                   .AlignCenter()
                   .Text(title)
                   .SemiBold()
                   .FontSize(14);

                // السطر الفرعي (الفترة / فلتر إضافي)
                if (!string.IsNullOrWhiteSpace(subtitle))
                {
                    col.Item()
                       .PaddingTop(4)
                       .AlignCenter()
                       .Text(subtitle)
                       .FontSize(11);
                }

                col.Item()
                   .PaddingTop(8)
                   .LineHorizontal(1);
            });
        }

        /// <summary>
        /// تذييل موحد: خط أفقي، شعار + اسم التطبيق، معلومات الاتصال، ورقم الصفحة.
        /// </summary>
        public static void RenderFooter(
            IContainer container,
            string pageLabel,   // مثلاً: "Page" أو "صفحة"
            bool isRtl)
        {
            container.Column(col =>
            {
                col.Spacing(4);

                // خط فوق التذييل
                col.Item()
                   .LineHorizontal(1);

                // سطر التذييل
                col.Item()
                   .PaddingTop(2)
                   .Row(row =>
                   {
                       // 1) يسار: لوجو Fixtroller + الاسم جنبه
                       row.RelativeItem()
                      .AlignLeft()
                      .Row(left =>
                      {
                          if (_appLogoBytes.Length > 0)
                          {
                              left.ConstantItem(17)      // 👈 كبّر العرض
                                  .Height(15)            // 👈 وكبّر الارتفاع
                                  .Image(_appLogoBytes);
                          }

                          left.RelativeItem()
                              .PaddingLeft(8)
                              .Text(t =>
                              {
                                  t.Span("Fixtroller")
                                   .SemiBold()
                                   .FontSize(10);
                              });
                      });

                       // 2) وسط: رقم + إيميل (بدون |)
                       row.RelativeItem()
                          .AlignCenter()
                          .Text(t =>
                          {
                              t.DefaultTextStyle(s => s.FontSize(10));

                              t.Span("+970 594 531 617  ");
                              t.Span("fixtroller.app@gmail.com");
                          });

                       // 3) يمين: رقم الصفحة / عدد الصفحات
                       row.RelativeItem()
                          .AlignRight()
                          .Text(txt =>
                          {
                              txt.DefaultTextStyle(s => s.FontSize(10));

                              txt.Span(pageLabel + " ");
                              txt.CurrentPageNumber();
                              txt.Span(" / ");
                              txt.TotalPages();
                          });
                   });
            });
        }
    }
}
