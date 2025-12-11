using Fixtroller.DAL.Data.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Fixtroller.BLL.Reports
{
    public class TechnicianPerformanceReportDocument : IDocument
    {
        private readonly TechnicianPerformanceReportDTO _model;

        public TechnicianPerformanceReportDocument(TechnicianPerformanceReportDTO model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header().Column(col =>
                {
                    // العنوان
                    col.Item()
                        .AlignCenter()
                        .Text(t =>
                        {
                            t.Span("تقرير أداء الفني")
                             .FontSize(18)
                             .SemiBold();
                        });

                    // اسم الفني + الكاتيجوري
                    col.Item()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("الفني: ").SemiBold();
                            text.Span(_model.TechnicianName ?? string.Empty);
                            if (!string.IsNullOrWhiteSpace(_model.TechnicianCategoryName))
                            {
                                text.Span(" (");
                                text.Span(_model.TechnicianCategoryName!);
                                text.Span(")");
                            }
                        });

                    // الفترة
                    col.Item()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("الفترة: ").SemiBold();
                            text.Span(_model.FromUtc.ToString("yyyy-MM-dd"));
                            text.Span("  إلى  ");
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
                        txt.Span("Fixtroller - Technician Performance Report  ");
                        txt.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                        txt.Span("  |  صفحة ");
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

                    col.Item().Text(t =>
                    {
                        t.Span("الأرقام العامة للفني")
                         .SemiBold()
                         .FontSize(14);
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("عدد الطلبات المعيّنة في الفترة: ").SemiBold();
                        t.Span(s.AssignedCount.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("عدد الطلبات التي أُغلقت: ").SemiBold();
                        t.Span(s.CompletedCount.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("عدد الطلبات المتأخرة (SLA): ").SemiBold();
                        t.Span(s.OverdueCount.ToString());
                    });

                    if (s.OverdueRate.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("نسبة الطلبات المتأخرة (من الطلبات ذات SLA): ").SemiBold();
                            t.Span($"{s.OverdueRate.Value:0.##}%");
                        });
                    }

                    if (s.AverageClosureHours.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("متوسط زمن الإغلاق: ").SemiBold();
                            t.Span($"{s.AverageClosureHours.Value:0.##} ساعة");
                        });
                    }

                    if (s.AverageStartDelayHours.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("متوسط زمن بدء العمل بعد التعيين: ").SemiBold();
                            t.Span($"{s.AverageStartDelayHours.Value:0.##} ساعة");
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
                    t.Span("لا توجد طلبات لهذا الفني ضمن الفترة المحددة.")
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span("تفاصيل الطلبات")
                     .SemiBold()
                     .FontSize(14);
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // #
                        columns.ConstantColumn(50);  // رقم الطلب
                        columns.ConstantColumn(70);  // تاريخ الإنشاء
                        columns.RelativeColumn();    // نوع المشكلة
                        columns.ConstantColumn(80);  // الحالة
                        columns.ConstantColumn(80);  // تعيين
                        columns.ConstantColumn(80);  // بدء عمل
                        columns.ConstantColumn(80);  // إغلاق
                        columns.ConstantColumn(70);  // SLA(س)
                        columns.ConstantColumn(70);  // متأخر؟
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("#").SemiBold());
                        header.Cell().Text(t => t.Span("رقم").SemiBold());
                        header.Cell().Text(t => t.Span("إنشاء").SemiBold());
                        header.Cell().Text(t => t.Span("نوع المشكلة").SemiBold());
                        header.Cell().Text(t => t.Span("الحالة").SemiBold());
                        header.Cell().Text(t => t.Span("تعيين").SemiBold());
                        header.Cell().Text(t => t.Span("بدء").SemiBold());
                        header.Cell().Text(t => t.Span("إغلاق").SemiBold());
                        header.Cell().Text(t => t.Span("SLA").SemiBold());
                        header.Cell().Text(t => t.Span("متأخر").SemiBold());
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
                            overdueText = item.IsOverdue.Value ? "نعم" : "لا";

                        table.Cell().Text(overdueText);

                        index++;
                    }
                });
            });
        }
    }
}
