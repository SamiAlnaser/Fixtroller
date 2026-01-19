using Fixtroller.DAL.Data.DTOs.Reports.Responses;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Text;                // جديد
using Fixtroller.BLL.Reports;     // جديد

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

        private bool IsRtl =>
            string.Equals(_language, "ar", StringComparison.OrdinalIgnoreCase);

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);

                // ✅ الهيدر: شعار + عنوان التقرير + الفترة + إجمالي المكتملة
                page.Header().Element(HeaderSection);

                // ✅ محتوى مع دعم RTL
                page.Content().Element(content =>
                {
                    var dirContainer = IsRtl
                        ? content.ContentFromRightToLeft()
                        : content.ContentFromLeftToRight();

                    dirContainer.Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Element(BucketsSection);
                        col.Item().Element(ProblemTypesSection);
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
            // العنوان الأساسي من الـ resources
            var title = T("Report.DurationByProblemType.Header.Title");

            // نبني السطر/السطور الفرعية (الفترة + إجمالي المكتملة)
            var sb = new StringBuilder();

            // الفترة: من .. إلى ..
            sb.Append(T("Report.Common.FromLabel"))
              .Append(": ")
              .Append(_model.FromUtc.ToString("yyyy-MM-dd"))
              .Append("   ")
              .Append(T("Report.Common.ToLabel"))
              .Append(": ")
              .Append(_model.ToUtc.ToString("yyyy-MM-dd"));

            // سطر جديد: إجمالي الطلبات المكتملة في الفترة
            sb.AppendLine();
            sb.Append(T("Report.DurationByProblemType.Header.TotalCompleted"))
              .Append(": ")
              .Append(_model.TotalCompleted);

            var subtitle = sb.ToString();

            // نرسم الهيدر عبر ReportBranding (الشعار + العنوان + السطر الفرعي)
            ReportBranding.RenderHeader(container, title, subtitle);
        }
        // =====================================================

        void BucketsSection(IContainer container)
        {
            if (_model.Buckets == null || _model.Buckets.Count == 0)
            {
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
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.Buckets.Header.Duration")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.Buckets.Header.Count")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.Buckets.Header.Percent")).SemiBold());
                    });

                    foreach (var b in _model.Buckets.OrderBy(b => b.BucketKey))
                    {
                        var durationLabel = b.BucketKey switch
                        {
                            "lt12h" => T("Report.DurationByProblemType.Buckets.Value.LessThan12Hours"),
                            "h12to72" => T("Report.DurationByProblemType.Buckets.Value.From12HoursTo3Days"),
                            "gt72h" => T("Report.DurationByProblemType.Buckets.Value.MoreThan3Days"),
                            _ => b.BucketName
                        };

                        table.Cell().Text(durationLabel);
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
                    t.Span(T("Report.DurationByProblemType.ProblemTypes.NoData"))
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

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
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.ProblemTypes.Header.ProblemType")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.ProblemTypes.Header.CompletedCount")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.ProblemTypes.Header.AvgClosureHours")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.DurationByProblemType.ProblemTypes.Header.OverdueRate")).SemiBold());
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
