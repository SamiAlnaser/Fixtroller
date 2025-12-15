using Fixtroller.DAL.Data.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

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

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header().Column(col =>
                {
                    // العنوان: "تقرير أداء الفني"
                    col.Item()
                        .AlignCenter()
                        .Text(t =>
                        {
                            t.Span(T("Report.TechPerformance.Header.Title"))
                             .FontSize(18)
                             .SemiBold();
                        });

                    // "الفني: {الاسم} (الفئة)"
                    col.Item()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(T("Report.TechPerformance.Header.TechnicianLabel") + ": ")
                                .SemiBold();
                            text.Span(_model.TechnicianName ?? string.Empty);

                            if (!string.IsNullOrWhiteSpace(_model.TechnicianCategoryName))
                            {
                                text.Span(" (");
                                text.Span(_model.TechnicianCategoryName!);
                                text.Span(")");
                            }
                        });

                    // الفترة: "الفترة: من ... إلى ..."
                    col.Item()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(T("Report.TechPerformance.Header.PeriodLabel") + ": ")
                                .SemiBold();

                            text.Span(T("Report.Common.FromLabel") + " ");
                            text.Span(_model.FromUtc.ToString("yyyy-MM-dd"));
                            text.Span("  ");
                            text.Span(T("Report.Common.ToLabel") + " ");
                            text.Span(_model.ToUtc.ToString("yyyy-MM-dd"));
                        });
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Element(SummarySection);
                    col.Item().Element(ItemsSection);
                });

                page.Footer()
                    .AlignCenter()
                    .Text(txt =>
                    {
                        // "Fixtroller - Technician Performance Report"
                        txt.Span(T("Report.TechPerformance.Footer.Text"));
                        txt.Span("  ");
                        txt.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                        txt.Span("  |  ");
                        // "صفحة"
                        txt.Span(T("Report.Common.PageLabel"));
                        txt.Span(" ");
                        txt.CurrentPageNumber();
                        txt.Span(" / ");
                        txt.TotalPages();
                    });
            });
        }

        void SummarySection(IContainer container)
        {
            var s = _model.Summary;

            container
                .Border(1)
                .Padding(8)
                .Column(col =>
                {
                    col.Spacing(4);

                    // "الأرقام العامة للفني"
                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.TechPerformance.Summary.Title"))
                         .SemiBold()
                         .FontSize(14);
                    });

                    col.Item().Text(t =>
                    {
                        // "عدد الطلبات المعيّنة في الفترة: "
                        t.Span(T("Report.TechPerformance.Summary.AssignedCount") + ": ")
                         .SemiBold();
                        t.Span(s.AssignedCount.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        // "عدد الطلبات التي أُغلقت: "
                        t.Span(T("Report.TechPerformance.Summary.CompletedCount") + ": ")
                         .SemiBold();
                        t.Span(s.CompletedCount.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        // "عدد الطلبات المتأخرة (SLA): "
                        t.Span(T("Report.TechPerformance.Summary.OverdueCount") + ": ")
                         .SemiBold();
                        t.Span(s.OverdueCount.ToString());
                    });

                    if (s.OverdueRate.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            // "نسبة الطلبات المتأخرة (من الطلبات ذات SLA): "
                            t.Span(T("Report.TechPerformance.Summary.OverdueRate") + ": ")
                             .SemiBold();
                            t.Span($"{s.OverdueRate.Value:0.##}%");
                        });
                    }

                    if (s.AverageClosureHours.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            // "متوسط زمن الإغلاق: "
                            t.Span(T("Report.TechPerformance.Summary.AverageClosureHours") + ": ")
                             .SemiBold();
                            t.Span($"{s.AverageClosureHours.Value:0.##} " +
                                   T("Report.Common.HoursSuffix")); // "ساعة"
                        });
                    }

                    if (s.AverageStartDelayHours.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            // "متوسط زمن بدء العمل بعد التعيين: "
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
                // "لا توجد طلبات لهذا الفني ضمن الفترة المحددة."
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

                // "تفاصيل الطلبات"
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
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.RequestId")).SemiBold());       // "رقم"
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.CreatedAt")).SemiBold());      // "إنشاء"
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.ProblemType")).SemiBold());    // "نوع المشكلة"
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.CaseType")).SemiBold());       // "الحالة"
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.AssignedAt")).SemiBold());     // "تعيين"
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.StartedAt")).SemiBold());      // "بدء"
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.ClosedAt")).SemiBold());       // "إغلاق"
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.SlaHours")).SemiBold());       // "SLA"
                        header.Cell().Text(t => t.Span(T("Report.TechPerformance.Items.Header.IsOverdue")).SemiBold());      // "متأخر"
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
                                ? T("Report.Common.Yes")   // "نعم"
                                : T("Report.Common.No");   // "لا"
                        }

                        table.Cell().Text(overdueText);

                        index++;
                    }
                });
            });
        }
    }

}
