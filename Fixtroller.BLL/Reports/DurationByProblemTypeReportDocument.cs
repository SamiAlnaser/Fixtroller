using Fixtroller.DAL.Data.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Fixtroller.BLL.Reports
{
    public class DurationByProblemTypeReportDocument : IDocument
    {
        private readonly DurationByProblemTypeReportDTO _model;

        public DurationByProblemTypeReportDocument(DurationByProblemTypeReportDTO model)
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
                            t.Span("تقرير التصنيفات حسب مدة الإغلاق ونوع المشكلة")
                             .FontSize(18)
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

                    // إجمالي المكتملة
                    col.Item()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("إجمالي الطلبات المكتملة في الفترة: ").SemiBold();
                            text.Span(_model.TotalCompleted.ToString());
                        });
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Element(BucketsSection);
                    col.Item().Element(ProblemTypesSection);
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Fixtroller - Duration by Problem Type Report  ");
                    txt.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                    txt.Span("  |  صفحة ");
                    txt.CurrentPageNumber();
                    txt.Span(" / ");
                    txt.TotalPages();
                });
            });
        }

        void BucketsSection(IContainer container)
        {
            if (_model.Buckets == null || _model.Buckets.Count == 0)
            {
                container.Text(t =>
                {
                    t.Span("لا توجد طلبات مكتملة ضمن الفترة المحددة.")
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                // عنوان القسم
                col.Item().Text(t =>
                {
                    t.Span("تقسيم الطلبات المكتملة حسب مدة الإغلاق")
                     .FontSize(14)
                     .SemiBold();
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();    // المدة
                        columns.ConstantColumn(80);  // العدد
                        columns.ConstantColumn(80);  // النسبة
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("مدة الإغلاق").SemiBold());
                        header.Cell().Text(t => t.Span("العدد").SemiBold());
                        header.Cell().Text(t => t.Span("النسبة %").SemiBold());
                    });

                    foreach (var b in _model.Buckets.OrderBy(b => b.BucketKey))
                    {
                        table.Cell().Text(b.BucketName);
                        table.Cell().Text(b.Count.ToString());
                        table.Cell().Text(b.Percentage.ToString("0.##"));
                    }
                });
            });
        }

        void ProblemTypesSection(IContainer container)
        {
            if (_model.ProblemTypes == null || _model.ProblemTypes.Count == 0)
            {
                container.Text(t =>
                {
                    t.Span("لا توجد بيانات لأنواع المشاكل ضمن الفترة المحددة.")
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                // عنوان القسم
                col.Item().Text(t =>
                {
                    t.Span("مؤشرات الأداء لكل نوع مشكلة")
                     .FontSize(14)
                     .SemiBold();
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // #
                        columns.RelativeColumn();    // نوع المشكلة
                        columns.ConstantColumn(80);  // عدد مكتملة
                        columns.ConstantColumn(110); // متوسط زمن الإغلاق
                        columns.ConstantColumn(110); // نسبة المتأخرة
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("#").SemiBold());
                        header.Cell().Text(t => t.Span("نوع المشكلة").SemiBold());
                        header.Cell().Text(t => t.Span("عدد مكتملة").SemiBold());
                        header.Cell().Text(t => t.Span("متوسط زمن الإغلاق (س)").SemiBold());
                        header.Cell().Text(t => t.Span("نسبة المتأخرة %").SemiBold());
                    });

                    int index = 1;
                    foreach (var p in _model.ProblemTypes.OrderByDescending(x => x.CompletedCount))
                    {
                        table.Cell().Text(index.ToString());
                        table.Cell().Text(p.ProblemTypeName);
                        table.Cell().Text(p.CompletedCount.ToString());

                        table.Cell().Text(
                            p.AverageClosureHours.HasValue
                                ? p.AverageClosureHours.Value.ToString("0.##")
                                : "-");

                        table.Cell().Text(
                            p.OverdueRate.HasValue
                                ? p.OverdueRate.Value.ToString("0.##")
                                : "-");

                        index++;
                    }
                });
            });
        }
    }
}
