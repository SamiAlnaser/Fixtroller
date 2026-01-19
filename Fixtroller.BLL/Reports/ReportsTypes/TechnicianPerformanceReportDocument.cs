using Fixtroller.DAL.Data.DTOs.Reports.Responses;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Text;              // ✅ جديد
using Fixtroller.BLL.Reports;   // ✅ جديد

namespace Fixtroller.BLL.Reports.ReportsTypes
{
    public class TechnicianPerformanceReportDocument : IDocument
    {
        private readonly TechnicianPerformanceReportDTO _model;
        private readonly IReportsTextBuilder _text;
        private readonly string _language;

        public TechnicianPerformanceReportDocument(
            TechnicianPerformanceReportDTO model,
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

                // ✅ هيدر موحّد: شعار + عنوان التقرير + الفني + الفترة
                page.Header().Element(HeaderSection);

                // ✅ اتجاه المحتوى حسب اللغة
                page.Content().Element(content =>
                {
                    var dirContainer = IsRtl
                        ? content.ContentFromRightToLeft()
                        : content.ContentFromLeftToRight();

                    dirContainer.Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Element(SummarySection);
                        col.Item().Element(ItemsSection);
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
            // العنوان الرئيسي: "تقرير أداء الفني"
            var title = T("Report.TechPerformance.Header.Title");

            var sb = new StringBuilder();

            // السطر الأول: "الفني: {الاسم} (الفئة)"
            sb.Append(T("Report.TechPerformance.Header.TechnicianLabel"))
              .Append(": ")
              .Append(_model.TechnicianName ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(_model.TechnicianCategoryName))
            {
                sb.Append(" (")
                  .Append(_model.TechnicianCategoryName)
                  .Append(")");
            }

            // سطر جديد
            sb.AppendLine();

            // السطر الثاني: "الفترة: من ... إلى ..."
            sb.Append(T("Report.TechPerformance.Header.PeriodLabel"))
              .Append(": ")
              .Append(T("Report.Common.FromLabel"))
              .Append(" ")
              .Append(_model.FromUtc.ToString("yyyy-MM-dd"))
              .Append("  ")
              .Append(T("Report.Common.ToLabel"))
              .Append(" ")
              .Append(_model.ToUtc.ToString("yyyy-MM-dd"));

            var subtitle = sb.ToString();

            // يرسم الشعار + العنوان + السطرين اللي فوق
            ReportBranding.RenderHeader(container, title, subtitle);
        }
        // =====================================================

        void SummarySection(IContainer container)
        {
            var s = _model.Summary;

            container
                .Border(1)
                .Padding(8)
                .Column(col =>
                {
                    col.Spacing(4);

                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.TechPerformance.Summary.Title"))
                         .SemiBold()
                         .FontSize(14);
                    });

                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.TechPerformance.Summary.AssignedCount") + ": ")
                         .SemiBold();
                        t.Span(s.AssignedCount.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.TechPerformance.Summary.CompletedCount") + ": ")
                         .SemiBold();
                        t.Span(s.CompletedCount.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.TechPerformance.Summary.OverdueCount") + ": ")
                         .SemiBold();
                        t.Span(s.OverdueCount.ToString());
                    });

                    if (s.OverdueRate.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span(T("Report.TechPerformance.Summary.OverdueRate") + ": ")
                             .SemiBold();
                            t.Span($"{s.OverdueRate.Value:0.##}%");
                        });
                    }

                    if (s.AverageClosureHours.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span(T("Report.TechPerformance.Summary.AverageClosureHours") + ": ")
                             .SemiBold();
                            t.Span($"{s.AverageClosureHours.Value:0.##} " +
                                   T("Report.Common.HoursSuffix"));
                        });
                    }

                    if (s.AverageStartDelayHours.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span(T("Report.TechPerformance.Summary.AverageStartDelayHours") + ": ")
                             .SemiBold();
                            t.Span($"{s.AverageStartDelayHours.Value:0.##} " +
                                   T("Report.Common.HoursSuffix"));
                        });
                    }
                });
        }

        void ItemsSection(IContainer container)
        {
            if (_model.Items == null || _model.Items.Count == 0)
            {
                container.Text(t =>
                {
                    t.Span(T("Report.TechPerformance.Items.NoData"))
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span(T("Report.TechPerformance.Items.Title"))
                     .SemiBold()
                     .FontSize(14);
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);  // #
                        columns.ConstantColumn(45);  // رقم
                        columns.ConstantColumn(55);  // إنشاء
                        columns.RelativeColumn();    // نوع المشكلة
                        columns.ConstantColumn(60);  // الحالة
                        columns.ConstantColumn(60);  // تعيين
                        columns.ConstantColumn(60);  // بدء
                        columns.ConstantColumn(60);  // إغلاق
                        columns.ConstantColumn(50);  // SLA
                        columns.ConstantColumn(50);  // متأخر
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("#").SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.RequestId")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.CreatedAt")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.ProblemType")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.CaseType")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.AssignedAt")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.StartedAt")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.ClosedAt")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.SlaHours")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.IsOverdue")).SemiBold());
                    });

                    int index = 1;
                    foreach (var item in _model.Items.OrderBy(i => i.CreatedAtUtc))
                    {
                        table.Cell().Text(index.ToString());
                        table.Cell().Text(item.RequestId.ToString());
                        table.Cell().Text(item.CreatedAtUtc.ToString("MM-dd"));

                        table.Cell().Text(item.ProblemTypeName);
                        table.Cell().Text(item.CaseTypeName);

                        table.Cell().Text(item.AssignedAtUtc.ToString("MM-dd HH:mm"));
                        table.Cell().Text(item.FirstWorkStartedAtUtc?.ToString("MM-dd HH:mm") ?? "-");
                        table.Cell().Text(item.ClosedAtUtc?.ToString("MM-dd HH:mm") ?? "-");

                        table.Cell().Text(item.ExpectedDurationHours?.ToString() ?? "-");

                        string overdueText = "-";
                        if (item.IsOverdue.HasValue)
                        {
                            overdueText = item.IsOverdue.Value
                                ? T("Report.Common.Yes")
                                : T("Report.Common.No");
                        }

                        table.Cell().Text(overdueText);

                        index++;
                    }
                });
            });
        }
    }
}
