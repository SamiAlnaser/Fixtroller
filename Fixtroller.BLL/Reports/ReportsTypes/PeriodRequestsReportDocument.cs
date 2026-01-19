using Fixtroller.DAL.Data.DTOs.Reports.Responses;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Text;


namespace Fixtroller.BLL.Reports.ReportsTypes
{
    public class PeriodRequestsReportDocument : IDocument
    {
        private readonly PeriodRequestsReportDTO _model;
        private readonly IReportsTextBuilder _text;
        private readonly string _language;

        public PeriodRequestsReportDocument(
            PeriodRequestsReportDTO model,
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

                // ✅ الهيدر الجديد: شعار + عنوان التقرير + الفترة (+ نوع المشكلة لو موجود)
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

        private void HeaderSection(IContainer container)
        {
            var title = T("Report.PeriodRequests.Header.Title");

            var sb = new StringBuilder();

            // الفترة
            sb.Append(T("Report.Common.FromLabel"))
              .Append(": ")
              .Append(_model.FromUtc.ToString("yyyy-MM-dd"))
              .Append("   ")
              .Append(T("Report.Common.ToLabel"))
              .Append(": ")
              .Append(_model.ToUtc.ToString("yyyy-MM-dd"));

            // نوع المشكلة لو في فلتر
            if (_model.ProblemTypeId is not null &&
                !string.IsNullOrWhiteSpace(_model.ProblemTypeName))
            {
                sb.AppendLine();
                sb.Append(T("Report.PeriodRequests.Header.ProblemTypeFilterLabel"))
                  .Append(": ")
                  .Append(_model.ProblemTypeName!);
            }

            var subtitle = sb.ToString();

            ReportBranding.RenderHeader(container, title, subtitle);
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
                        t.Span(T("Report.PeriodRequests.Summary.Title"))
                         .FontSize(14)
                         .SemiBold();
                    });

                    col.Item().Text(text =>
                    {
                        text.Span(T("Report.PeriodRequests.Summary.TotalRequests") + ": ").SemiBold();
                        text.Span(_model.Summary.TotalRequests.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span(T("Report.PeriodRequests.Summary.CompletedCount") + ": ").SemiBold();
                        text.Span(_model.Summary.CompletedCount.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span(T("Report.PeriodRequests.Summary.OpenCount") + ": ").SemiBold();
                        text.Span(_model.Summary.OpenCount.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span(T("Report.PeriodRequests.Summary.CancelledCount") + ": ").SemiBold();
                        text.Span(_model.Summary.CancelledCount.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span(T("Report.PeriodRequests.Summary.OverdueCount") + ": ").SemiBold();
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
                    t.Span(T("Report.PeriodRequests.Items.Empty"))
                     .Italic();
                });

                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span(T("Report.PeriodRequests.Items.Title"))
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
                        columns.ConstantColumn(70);  // SLA
                    });

                    // العناوين
                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span(T("Report.PeriodRequests.Items.Header.RequestId")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.PeriodRequests.Items.Header.Date")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.PeriodRequests.Items.Header.ProblemType")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.PeriodRequests.Items.Header.Status")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.PeriodRequests.Items.Header.MainTechnician")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.PeriodRequests.Items.Header.ClosedAt")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.PeriodRequests.Items.Header.Sla")).SemiBold());
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
                        {
                            slaText = "-";
                        }
                        else
                        {
                            slaText = item.IsWithinSla.Value
                                ? T("Report.PeriodRequests.SlaStatus.Within")
                                : T("Report.PeriodRequests.SlaStatus.Late");
                        }

                        table.Cell().Text(slaText);
                    }
                });
            });
        }
    }
}
