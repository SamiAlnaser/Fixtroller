using Fixtroller.DAL.Data.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Fixtroller.BLL.Reports.ReportsTypes
{
    public class DurationByProblemTypeReportDocument : IDocument
    {
        private readonly DurationByProblemTypeReportDTO _model;
        private readonly IReportsTextBuilder _text;
        private readonly string _language;

        public DurationByProblemTypeReportDocument(
            DurationByProblemTypeReportDTO model,
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
                    // العنوان: "تقرير التصنيفات حسب مدة الإغلاق ونوع المشكلة"
                    col.Item()
                        .AlignCenter()
                        .Text(t =>
                        {
                            t.Span(T("Report.DurationByProblemType.Header.Title"))
                             .FontSize(18)
                             .SemiBold();
                        });

                    // الفترة: "من: {0} إلى: {1}"
                    col.Item()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(T("Report.Common.FromLabel") + ": ").SemiBold();
                            text.Span(_model.FromUtc.ToString("yyyy-MM-dd"));
                            text.Span("   ");
                            text.Span(T("Report.Common.ToLabel") + ": ").SemiBold();
                            text.Span(_model.ToUtc.ToString("yyyy-MM-dd"));
                        });

                    // إجمالي المكتملة: "إجمالي الطلبات المكتملة في الفترة: {0}"
                    col.Item()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(T("Report.DurationByProblemType.Header.TotalCompleted") + ": ")
                                .SemiBold();
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
                    // "Fixtroller - Duration by Problem Type Report"
                    txt.Span(T("Report.DurationByProblemType.Footer.Text"));
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

        void BucketsSection(IContainer container)
        {
            if (_model.Buckets == null || _model.Buckets.Count == 0)
            {
                // "لا توجد طلبات مكتملة ضمن الفترة المحددة."
                container.Text(t =>
                {
                    t.Span(T("Report.DurationByProblemType.Buckets.NoData"))
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                // عنوان القسم: "تقسيم الطلبات المكتملة حسب مدة الإغلاق"
                col.Item().Text(t =>
                {
                    t.Span(T("Report.DurationByProblemType.Buckets.Title"))
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
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.Buckets.Header.Duration")).SemiBold());   // "مدة الإغلاق"
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.Buckets.Header.Count")).SemiBold());      // "العدد"
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.Buckets.Header.Percent")).SemiBold());    // "النسبة %"
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
                // "لا توجد بيانات لأنواع المشاكل ضمن الفترة المحددة."
                container.Text(t =>
                {
                    t.Span(T("Report.DurationByProblemType.ProblemTypes.NoData"))
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                // عنوان القسم: "مؤشرات الأداء لكل نوع مشكلة"
                col.Item().Text(t =>
                {
                    t.Span(T("Report.DurationByProblemType.ProblemTypes.Title"))
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
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.ProblemTypes.Header.ProblemType")).SemiBold());  // "نوع المشكلة"
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.ProblemTypes.Header.CompletedCount")).SemiBold()); // "عدد مكتملة"
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.ProblemTypes.Header.AvgClosureHours")).SemiBold()); // "متوسط زمن الإغلاق (س)"
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.ProblemTypes.Header.OverdueRate")).SemiBold());    // "نسبة المتأخرة %"
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
