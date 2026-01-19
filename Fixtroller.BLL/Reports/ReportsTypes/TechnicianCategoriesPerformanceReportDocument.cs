using Fixtroller.DAL.Data.DTOs.Reports.Responses;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Text;              // ✅ جديد
using Fixtroller.BLL.Reports;   // ✅ جديد

namespace Fixtroller.BLL.Reports.ReportsTypes
{
    public class TechnicianCategoriesPerformanceReportDocument : IDocument
    {
        private readonly TechnicianCategoriesPerformanceReportDTO _model;
        private readonly IReportsTextBuilder _text;
        private readonly string _language;

        public TechnicianCategoriesPerformanceReportDocument(
            TechnicianCategoriesPerformanceReportDTO model,
            IReportsTextBuilder text,
            string language)
        {
            _model = model;
            _text = text;
            _language = language;
        }

        private string T(string key, params object[] args)
            => _text.Get(key, _language, args);

        private bool IsRtl =>
            string.Equals(_language, "ar", StringComparison.OrdinalIgnoreCase);

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);

                // ✅ هيدر موحّد: شعار + عنوان التقرير + الفترة
                page.Header().Element(HeaderSection);

                // ✅ اتجاه المحتوى حسب اللغة
                page.Content().Element(content =>
                {
                    var dirContainer = IsRtl
                        ? content.ContentFromRightToLeft()
                        : content.ContentFromLeftToRight();

                    dirContainer.Column(col =>
                    {
                        col.Spacing(15);

                        if (_model.Categories == null || _model.Categories.Count == 0)
                        {
                            // "لا توجد بيانات للفنيين ضمن الفترة المحددة."
                            col.Item().Text(t =>
                            {
                                t.Span(T("Report.TechCategories.NoData"))
                                 .Italic();
                            });
                            return;
                        }

                        foreach (var cat in _model.Categories.OrderByDescending(c => c.TotalAssigned))
                        {
                            col.Item().Element(cn => CategorySection(cn, cat));
                        }
                    });
                });

                page.Footer().Element(footer =>
                {
                    ReportBranding.RenderFooter(
                        footer,
                        T("Report.Common.PageLabel"), // "Page" / "صفحة"
                        IsRtl);
                });
            });
        }

        // ================== الهيدر الموحّد ==================
        private void HeaderSection(IContainer container)
        {
            // العنوان: "تقرير الفنيين حسب الفئة (Category)"
            var title = T("Report.TechCategories.Header.Title");

            // السطر الفرعي: "الفترة: من {from} إلى {to}"
            var sb = new StringBuilder();

            sb.Append(T("Report.TechCategories.Header.PeriodLabel"))
              .Append(": ")
              .Append(T("Report.Common.FromLabel"))
              .Append(" ")
              .Append(_model.FromUtc.ToString("yyyy-MM-dd"))
              .Append("  ")
              .Append(T("Report.Common.ToLabel"))
              .Append(" ")
              .Append(_model.ToUtc.ToString("yyyy-MM-dd"));

            var subtitle = sb.ToString();

            ReportBranding.RenderHeader(container, title, subtitle);
        }
        // =====================================================

        void CategorySection(IContainer container, TechnicianCategoryPerformanceDTO cat)
        {
            container.Column(col =>
            {
                col.Spacing(6);

                // عنوان الفئة = اسم الفئة من الداتا
                col.Item().Text(t =>
                {
                    t.Span(cat.CategoryName)
                     .FontSize(14)
                     .SemiBold();
                });

                // ملخص الفئة
                col.Item()
                    .Border(1)
                    .Padding(6)
                    .Column(c2 =>
                    {
                        c2.Spacing(3);

                        c2.Item().Text(t =>
                        {
                            t.Span(T("Report.TechCategories.Summary.TechniciansCount") + ": ")
                             .SemiBold();
                            t.Span(cat.TechniciansCount.ToString());
                        });

                        c2.Item().Text(t =>
                        {
                            t.Span(T("Report.TechCategories.Summary.TotalAssigned") + ": ")
                             .SemiBold();
                            t.Span(cat.TotalAssigned.ToString());
                        });

                        c2.Item().Text(t =>
                        {
                            t.Span(T("Report.TechCategories.Summary.TotalCompleted") + ": ")
                             .SemiBold();
                            t.Span(cat.TotalCompleted.ToString());
                        });

                        c2.Item().Text(t =>
                        {
                            t.Span(T("Report.TechCategories.Summary.TotalOverdue") + ": ")
                             .SemiBold();
                            t.Span(cat.TotalOverdue.ToString());
                        });

                        if (cat.CompletionRate.HasValue)
                        {
                            c2.Item().Text(t =>
                            {
                                t.Span(T("Report.TechCategories.Summary.CompletionRate") + ": ")
                                 .SemiBold();
                                t.Span($"{cat.CompletionRate.Value:0.##}%");
                            });
                        }

                        if (cat.OverdueRate.HasValue)
                        {
                            c2.Item().Text(t =>
                            {
                                t.Span(T("Report.TechCategories.Summary.OverdueRate") + ": ")
                                 .SemiBold();
                                t.Span($"{cat.OverdueRate.Value:0.##}%");
                            });
                        }

                        if (cat.AverageClosureHours.HasValue)
                        {
                            c2.Item().Text(t =>
                            {
                                t.Span(T("Report.TechCategories.Summary.AverageClosureHours") + ": ")
                                 .SemiBold();
                                t.Span($"{cat.AverageClosureHours.Value:0.##} " +
                                       T("Report.Common.HoursSuffix"));
                            });
                        }

                        if (cat.AverageRequestsPerTechnician.HasValue)
                        {
                            c2.Item().Text(t =>
                            {
                                t.Span(T("Report.TechCategories.Summary.AverageRequestsPerTechnician") + ": ")
                                 .SemiBold();
                                t.Span($"{cat.AverageRequestsPerTechnician.Value:0.##}");
                            });
                        }
                    });

                // جدول الفنيين داخل الفئة
                if (cat.Technicians != null && cat.Technicians.Count > 0)
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  // #
                            columns.RelativeColumn();    // الاسم
                            columns.ConstantColumn(80);  // Assigned
                            columns.ConstantColumn(80);  // Completed
                            columns.ConstantColumn(80);  // Overdue
                            columns.ConstantColumn(110); // Avg Closure
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text(t => t.Span("#").SemiBold());
                            header.Cell().Text(t => t.Span(T("Report.TechCategories.Table.Header.Technician")).SemiBold());
                            header.Cell().Text(t => t.Span(T("Report.TechCategories.Table.Header.Assigned")).SemiBold());
                            header.Cell().Text(t => t.Span(T("Report.TechCategories.Table.Header.Completed")).SemiBold());
                            header.Cell().Text(t => t.Span(T("Report.TechCategories.Table.Header.Overdue")).SemiBold());
                            header.Cell().Text(t => t.Span(T("Report.TechCategories.Table.Header.AvgClosureHours")).SemiBold());
                        });

                        int index = 1;
                        foreach (var t in cat.Technicians.OrderByDescending(x => x.AssignedCount))
                        {
                            table.Cell().Text(index.ToString());
                            table.Cell().Text(t.TechnicianName);
                            table.Cell().Text(t.AssignedCount.ToString());
                            table.Cell().Text(t.CompletedCount.ToString());
                            table.Cell().Text(t.OverdueCount.ToString());
                            table.Cell().Text(
                                t.AverageClosureHours.HasValue
                                    ? t.AverageClosureHours.Value.ToString("0.##")
                                    : "-");
                            index++;
                        }
                    });
                }
                else
                {
                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.TechCategories.Category.NoTechnicians"))
                         .Italic();
                    });
                }
            });
        }
    }
}
