using Fixtroller.DAL.Data.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Fixtroller.BLL.Reports
{
    public class PeriodRequestsReportDocument : IDocument
    {
        private readonly PeriodRequestsReportDTO _model;

        public PeriodRequestsReportDocument(PeriodRequestsReportDTO model)
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
                    // العنوان الرئيسي
                    col.Item()
                        .AlignCenter()
                        .Text(t =>
                        {
                            t.Span("تقرير الطلبات لفترة زمنية")
                             .FontSize(20)
                             .SemiBold();
                        });

                    // الفترة
                    col.Item()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("من: ").SemiBold();
                            text.Span(_model.FromUtc.ToString("yyyy-MM-dd"));
                            text.Span("   إلى: ").SemiBold();
                            text.Span(_model.ToUtc.ToString("yyyy-MM-dd"));
                        });

                    // نوع المشكلة (إن وجد)
                    if (_model.ProblemTypeId is not null &&
                        !string.IsNullOrWhiteSpace(_model.ProblemTypeName))
                    {
                        col.Item()
                           .AlignCenter()
                           .Text(text =>
                           {
                               text.Span("نوع المشكلة المفلتر: ").SemiBold();
                               text.Span(_model.ProblemTypeName!);
                           });
                    }
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Element(SummarySection);
                    col.Item().Element(ItemsSection);
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Fixtroller - Period Requests Report  ");
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
            container
                .Border(1)
                .Padding(8)
                .Column(col =>
                {
                    col.Spacing(4);

                    col.Item().Text(t =>
                    {
                        t.Span("الأرقام العامة")
                         .FontSize(14)
                         .SemiBold();
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("إجمالي الطلبات: ").SemiBold();
                        text.Span(_model.Summary.TotalRequests.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("عدد المكتملة: ").SemiBold();
                        text.Span(_model.Summary.CompletedCount.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("عدد المفتوحة: ").SemiBold();
                        text.Span(_model.Summary.OpenCount.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("عدد الملغاة: ").SemiBold();
                        text.Span(_model.Summary.CancelledCount.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("عدد المتأخرة (SLA): ").SemiBold();
                        text.Span(_model.Summary.OverdueCount.ToString());
                    });
                });
        }

        void ItemsSection(IContainer container)
        {
            if (_model.Items == null || _model.Items.Count == 0)
            {
                container.Text(t =>
                {
                    t.Span("لا توجد طلبات ضمن الفترة المحددة.")
                     .Italic();
                });

                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span("قائمة الطلبات")
                     .FontSize(14)
                     .SemiBold();
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // #
                        columns.ConstantColumn(70);  // التاريخ
                        columns.RelativeColumn();    // نوع المشكلة
                        columns.ConstantColumn(80);  // الحالة
                        columns.ConstantColumn(110); // الفني الرئيسي
                        columns.ConstantColumn(80);  // تاريخ الإغلاق
                        columns.ConstantColumn(70);  // داخل SLA؟
                    });

                    // العناوين
                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("رقم").SemiBold());
                        header.Cell().Text(t => t.Span("تاريخ").SemiBold());
                        header.Cell().Text(t => t.Span("نوع المشكلة").SemiBold());
                        header.Cell().Text(t => t.Span("الحالة").SemiBold());
                        header.Cell().Text(t => t.Span("الفني الرئيسي").SemiBold());
                        header.Cell().Text(t => t.Span("إغلاق").SemiBold());
                        header.Cell().Text(t => t.Span("SLA").SemiBold());
                    });

                    // الصفوف
                    foreach (var item in _model.Items.OrderBy(i => i.CreatedAtUtc))
                    {
                        table.Cell().Text(item.RequestId.ToString());
                        table.Cell().Text(item.CreatedAtUtc.ToString("MM-dd"));

                        table.Cell().Text(item.ProblemTypeName);
                        table.Cell().Text(item.CaseTypeName);

                        table.Cell().Text(string.IsNullOrWhiteSpace(item.MainTechnicianName)
                            ? "-"
                            : item.MainTechnicianName);

                        table.Cell().Text(item.ClosedAtUtc?.ToString("MM-dd") ?? "-");

                        string slaText;
                        if (item.IsWithinSla is null)
                            slaText = "-";
                        else
                            slaText = item.IsWithinSla.Value ? "داخل" : "متأخر";

                        table.Cell().Text(slaText);
                    }
                });
            });
        }
    }
}
