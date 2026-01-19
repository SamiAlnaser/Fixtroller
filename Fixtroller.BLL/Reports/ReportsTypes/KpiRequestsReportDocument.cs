using Fixtroller.DAL.Data.DTOs.Reports.Responses;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Text;              // ✅ جديد
using Fixtroller.BLL.Reports;   // ✅ جديد

namespace Fixtroller.BLL.Reports.ReportsTypes
{
    public class KpiRequestsReportDocument : IDocument
    {
        private readonly KpiRequestsReportDTO _model;
        private readonly IReportsTextBuilder _text;
        private readonly string _language;

        public KpiRequestsReportDocument(KpiRequestsReportDTO model, IReportsTextBuilder text, string language)
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

                // ✅ هيدر موحّد: شعار + عنوان التقرير + الفترة + (نوع مشكلة لو فيه فلتر)
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
                        col.Item().Element(TopProblemTypesSection);
                        col.Item().Element(TopDepartmentsSection);
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
            // العنوان الرئيسي
            var title = T("Report.KpiRequests.Header.Title");

            // نبني السطر/السطور الفرعية (الفترة + نوع المشكلة لو فيه)
            var sb = new StringBuilder();

            // الفترة: "من: {0}   إلى: {1}"
            sb.Append(T("Report.Common.FromLabel"))
              .Append(": ")
              .Append(_model.FromUtc.ToString("yyyy-MM-dd"))
              .Append("   ")
              .Append(T("Report.Common.ToLabel"))
              .Append(": ")
              .Append(_model.ToUtc.ToString("yyyy-MM-dd"));

            // نوع المشكلة لو فيه فلتر
            if (_model.ProblemTypeId is not null &&
                !string.IsNullOrWhiteSpace(_model.ProblemTypeName))
            {
                sb.AppendLine();
                sb.Append(T("Report.KpiRequests.Header.ProblemTypeFilterLabel"))
                  .Append(": ")
                  .Append(_model.ProblemTypeName!);
            }

            var subtitle = sb.ToString();

            // رسم الهيدر بالشعار + العنوان + السطر الفرعي
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

                    // "الأرقام العامة"
                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.KpiRequests.Summary.Title"))
                         .FontSize(14)
                         .SemiBold();
                    });

                    col.Item().Text(text =>
                    {
                        text.Span(T("Report.KpiRequests.Summary.TotalRequests") + ": ")
                            .SemiBold();
                        text.Span(s.TotalRequests.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span(T("Report.KpiRequests.Summary.NewRequests") + ": ")
                            .SemiBold();
                        text.Span(s.NewRequests.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span(T("Report.KpiRequests.Summary.ClosedRequests") + ": ")
                            .SemiBold();
                        text.Span(s.ClosedRequests.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span(T("Report.KpiRequests.Summary.RemainingRequests") + ": ")
                            .SemiBold();
                        text.Span(s.RemainingRequests.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span(T("Report.KpiRequests.Summary.OverdueRequests") + ": ")
                            .SemiBold();
                        text.Span(s.OverdueRequests.ToString());
                    });

                    if (s.CompletionRate.HasValue)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span(T("Report.KpiRequests.Summary.CompletionRate") + ": ")
                                .SemiBold();
                            text.Span($"{s.CompletionRate.Value:0.##}%");
                        });
                    }

                    if (s.OverdueRate.HasValue)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span(T("Report.KpiRequests.Summary.OverdueRate") + ": ")
                                .SemiBold();
                            text.Span($"{s.OverdueRate.Value:0.##}%");
                        });
                    }

                    if (s.SlaComplianceRate.HasValue)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span(T("Report.KpiRequests.Summary.SlaComplianceRate") + ": ")
                                .SemiBold();
                            text.Span($"{s.SlaComplianceRate.Value:0.##}%");
                        });
                    }

                    if (s.AverageClosureHours.HasValue)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span(T("Report.KpiRequests.Summary.AverageClosureHours") + ": ")
                                .SemiBold();
                            text.Span($"{s.AverageClosureHours.Value:0.##} " +
                                      T("Report.Common.HoursSuffix"));
                        });
                    }
                });
        }

        void TopProblemTypesSection(IContainer container)
        {
            if (_model.TopProblemTypes == null || _model.TopProblemTypes.Count == 0)
            {
                container.Text(t =>
                {
                    t.Span(T("Report.KpiRequests.TopProblemTypes.NoData"))
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span(T("Report.KpiRequests.TopProblemTypes.Title"))
                     .FontSize(14)
                     .SemiBold();
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // #
                        columns.RelativeColumn();    // الاسم
                        columns.ConstantColumn(60);  // العدد
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("#").SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.KpiRequests.TopProblemTypes.Header.ProblemType")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.KpiRequests.TopProblemTypes.Header.Count")).SemiBold());
                    });

                    int index = 1;
                    foreach (var item in _model.TopProblemTypes.OrderByDescending(x => x.Count))
                    {
                        table.Cell().Text(index.ToString());
                        table.Cell().Text(item.ProblemTypeName);
                        table.Cell().Text(item.Count.ToString());
                        index++;
                    }
                });
            });
        }

        void TopDepartmentsSection(IContainer container)
        {
            if (_model.TopDepartments == null || _model.TopDepartments.Count == 0)
            {
                container.Text(t =>
                {
                    t.Span(T("Report.KpiRequests.TopDepartments.NoData"))
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span(T("Report.KpiRequests.TopDepartments.Title"))
                     .FontSize(14)
                     .SemiBold();
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // #
                        columns.RelativeColumn();    // القسم
                        columns.ConstantColumn(60);  // العدد
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("#").SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.KpiRequests.TopDepartments.Header.Department")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.KpiRequests.TopDepartments.Header.Count")).SemiBold());
                    });

                    int index = 1;
                    foreach (var item in _model.TopDepartments.OrderByDescending(x => x.Count))
                    {
                        table.Cell().Text(index.ToString());
                        table.Cell().Text(item.DepartmentName);
                        table.Cell().Text(item.Count.ToString());
                        index++;
                    }
                });
            });
        }
    }
}
