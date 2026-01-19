using Fixtroller.DAL.Data.DTOs.Reports.Responses;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Text;              // ✅ جديد
using Fixtroller.BLL.Reports;   // ✅ جديد

namespace Fixtroller.BLL.Reports.ReportsTypes
{
    public class MaintenanceDepartmentReportDocument : IDocument
    {
        private readonly MaintenanceDepartmentReportDTO _model;
        private readonly IReportsTextBuilder _text;
        private readonly string _language;

        public MaintenanceDepartmentReportDocument(
            MaintenanceDepartmentReportDTO model,
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
                        col.Spacing(12);

                        col.Item().Element(SummarySection);
                        col.Item().Element(CategoriesSection);
                        col.Item().Element(TopProblemTypesSection);
                        col.Item().Element(TopCategoriesSection);
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
            // العنوان: "تقرير قسم الصيانة ككل"
            var title = T("Report.MaintenanceDepartment.Header.Title");

            // نبني السطر الفرعي بنفس الفكرة القديمة:
            // "الفترة: من {from} إلى {to}"
            var sb = new StringBuilder();

            sb.Append(T("Report.MaintenanceDepartment.Header.PeriodLabel"))
              .Append(": ")
              .Append(T("Report.Common.FromLabel"))
              .Append(" ")
              .Append(_model.FromUtc.ToString("yyyy-MM-dd"))
              .Append("  ")
              .Append(T("Report.Common.ToLabel"))
              .Append(" ")
              .Append(_model.ToUtc.ToString("yyyy-MM-dd"));

            var subtitle = sb.ToString();

            // رسم الهيدر: الشعار + العنوان + السطر الفرعي
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
                        t.Span(T("Report.MaintenanceDepartment.Summary.Title"))
                         .SemiBold()
                         .FontSize(14);
                    });

                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.MaintenanceDepartment.Summary.TotalRequests") + ": ")
                         .SemiBold();
                        t.Span(s.TotalRequests.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.MaintenanceDepartment.Summary.NewRequests") + ": ")
                         .SemiBold();
                        t.Span(s.NewRequests.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.MaintenanceDepartment.Summary.ClosedRequests") + ": ")
                         .SemiBold();
                        t.Span(s.ClosedRequests.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.MaintenanceDepartment.Summary.RemainingRequests") + ": ")
                         .SemiBold();
                        t.Span(s.RemainingRequests.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.MaintenanceDepartment.Summary.OverdueRequests") + ": ")
                         .SemiBold();
                        t.Span(s.OverdueRequests.ToString());
                    });

                    if (s.CompletionRate.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span(T("Report.MaintenanceDepartment.Summary.CompletionRate") + ": ")
                             .SemiBold();
                            t.Span($"{s.CompletionRate.Value:0.##}%");
                        });
                    }

                    if (s.OverdueRate.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span(T("Report.MaintenanceDepartment.Summary.OverdueRate") + ": ")
                             .SemiBold();
                            t.Span($"{s.OverdueRate.Value:0.##}%");
                        });
                    }

                    if (s.SlaComplianceRate.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span(T("Report.MaintenanceDepartment.Summary.SlaComplianceRate") + ": ")
                             .SemiBold();
                            t.Span($"{s.SlaComplianceRate.Value:0.##}%");
                        });
                    }

                    if (s.AverageClosureHours.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span(T("Report.MaintenanceDepartment.Summary.AverageClosureHours") + ": ")
                             .SemiBold();
                            t.Span($"{s.AverageClosureHours.Value:0.##} " +
                                   T("Report.Common.HoursSuffix"));
                        });
                    }

                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.MaintenanceDepartment.Summary.TotalTechnicians") + ": ")
                         .SemiBold();
                        t.Span(_model.TotalTechnicians.ToString());
                    });
                });
        }

        void CategoriesSection(IContainer container)
        {
            if (_model.Categories == null || _model.Categories.Count == 0)
            {
                container.Text(t =>
                {
                    t.Span(T("Report.MaintenanceDepartment.Categories.NoData"))
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span(T("Report.MaintenanceDepartment.Categories.Title"))
                     .SemiBold()
                     .FontSize(14);
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.RelativeColumn();
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(100);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("#").SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.MaintenanceDepartment.Categories.Header.Category")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.MaintenanceDepartment.Categories.Header.TechniciansCount")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.MaintenanceDepartment.Categories.Header.RequestsCount")).SemiBold());
                    });

                    int index = 1;
                    foreach (var c in _model.Categories.OrderByDescending(c => c.RequestsCount))
                    {
                        table.Cell().Text(index.ToString());
                        table.Cell().Text(c.CategoryName);
                        table.Cell().Text(c.TechniciansCount.ToString());
                        table.Cell().Text(c.RequestsCount.ToString());
                        index++;
                    }
                });
            });
        }

        void TopProblemTypesSection(IContainer container)
        {
            if (_model.TopProblemTypes == null || _model.TopProblemTypes.Count == 0)
            {
                container.Text(t =>
                {
                    t.Span(T("Report.MaintenanceDepartment.TopProblemTypes.NoData"))
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span(T("Report.MaintenanceDepartment.TopProblemTypes.Title"))
                     .SemiBold()
                     .FontSize(14);
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.RelativeColumn();
                        columns.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("#").SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.MaintenanceDepartment.TopProblemTypes.Header.ProblemType")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.MaintenanceDepartment.TopProblemTypes.Header.RequestsCount")).SemiBold());
                    });

                    int index = 1;
                    foreach (var p in _model.TopProblemTypes.OrderByDescending(x => x.Count))
                    {
                        table.Cell().Text(index.ToString());
                        table.Cell().Text(p.ProblemTypeName);
                        table.Cell().Text(p.Count.ToString());
                        index++;
                    }
                });
            });
        }

        void TopCategoriesSection(IContainer container)
        {
            if (_model.TopCategoriesByRequests == null || _model.TopCategoriesByRequests.Count == 0)
            {
                container.Text(t =>
                {
                    t.Span(T("Report.MaintenanceDepartment.TopCategories.NoData"))
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span(T("Report.MaintenanceDepartment.TopCategories.Title"))
                     .SemiBold()
                     .FontSize(14);
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.RelativeColumn();
                        columns.ConstantColumn(100);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("#").SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.MaintenanceDepartment.TopCategories.Header.Category")).SemiBold());
                        header.Cell().Text(t => t.Span(T("Report.MaintenanceDepartment.TopCategories.Header.RequestsCount")).SemiBold());
                    });

                    int index = 1;
                    foreach (var c in _model.TopCategoriesByRequests.OrderByDescending(x => x.RequestsCount))
                    {
                        table.Cell().Text(index.ToString());
                        table.Cell().Text(c.CategoryName);
                        table.Cell().Text(c.RequestsCount.ToString());
                        index++;
                    }
                });
            });
        }
    }
}
