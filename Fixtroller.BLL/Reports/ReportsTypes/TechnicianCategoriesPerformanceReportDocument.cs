using Fixtroller.DAL.Data.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Fixtroller.BLL.Reports.ReportsTypes
{
    public class TechnicianCategoriesPerformanceReportDocument : IDocument
    {
        private readonly TechnicianCategoriesPerformanceReportDTO _model;
        private readonly IReportsTextBuilder _text;
        private readonly string _language;

        public TechnicianCategoriesPerformanceReportDocument(
            TechnicianCategoriesPerformanceReportDTO model,
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
                    // العنوان: "تقرير الفنيين حسب الفئة (Category)"
                    col.Item()
                        .AlignCenter()
                        .Text(t =>
                        {
                            t.Span(T("Report.TechCategories.Header.Title"))
                             .FontSize(18)
                             .SemiBold();
                        });

                    // الفترة: "الفترة: من {from} إلى {to}"
                    col.Item()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(T("Report.TechCategories.Header.PeriodLabel") + ": ")
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
                    col.Spacing(15);

                    if (_model.Categories == null || _model.Categories.Count == 0)
                    {
                        // "لا توجد بيانات للفنيين ضمن الفترة المحددة."
                        col.Item().Text(t =>
                        {
                            t.Span(T("Report.TechCategories.NoData"))
                             .Italic();
                        });
                        return;
                    }

                    foreach (var cat in _model.Categories.OrderByDescending(c => c.TotalAssigned))
                    {
                        col.Item().Element(cn => CategorySection(cn, cat));
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text(txt =>
                    {
                        // "Fixtroller - Technicians by Category Report"
                        txt.Span(T("Report.TechCategories.Footer.Text"));
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

        void CategorySection(IContainer container, TechnicianCategoryPerformanceDTO cat)
        {
            container.Column(col =>
            {
                col.Spacing(6);

                // عنوان الفئة = اسم الفئة من الداتا
                col.Item().Text(t =>
                {
                    t.Span(cat.CategoryName)
                     .FontSize(14)
                     .SemiBold();
                });

                // ملخص الفئة
                col.Item()
                    .Border(1)
                    .Padding(6)
                    .Column(c2 =>
                    {
                        c2.Spacing(3);

                        c2.Item().Text(t =>
                        {
                            // "عدد الفنيين: "
                            t.Span(T("Report.TechCategories.Summary.TechniciansCount") + ": ")
                             .SemiBold();
                            t.Span(cat.TechniciansCount.ToString());
                        });

                        c2.Item().Text(t =>
                        {
                            // "عدد الطلبات في الفترة: "
                            t.Span(T("Report.TechCategories.Summary.TotalAssigned") + ": ")
                             .SemiBold();
                            t.Span(cat.TotalAssigned.ToString());
                        });

                        c2.Item().Text(t =>
                        {
                            // "عدد الطلبات المكتملة: "
                            t.Span(T("Report.TechCategories.Summary.TotalCompleted") + ": ")
                             .SemiBold();
                            t.Span(cat.TotalCompleted.ToString());
                        });

                        c2.Item().Text(t =>
                        {
                            // "عدد الطلبات المتأخرة: "
                            t.Span(T("Report.TechCategories.Summary.TotalOverdue") + ": ")
                             .SemiBold();
                            t.Span(cat.TotalOverdue.ToString());
                        });

                        if (cat.CompletionRate.HasValue)
                        {
                            c2.Item().Text(t =>
                            {
                                // "نسبة الإنجاز: "
                                t.Span(T("Report.TechCategories.Summary.CompletionRate") + ": ")
                                 .SemiBold();
                                t.Span($"{cat.CompletionRate.Value:0.##}%");
                            });
                        }

                        if (cat.OverdueRate.HasValue)
                        {
                            c2.Item().Text(t =>
                            {
                                // "نسبة التأخير: "
                                t.Span(T("Report.TechCategories.Summary.OverdueRate") + ": ")
                                 .SemiBold();
                                t.Span($"{cat.OverdueRate.Value:0.##}%");
                            });
                        }

                        if (cat.AverageClosureHours.HasValue)
                        {
                            c2.Item().Text(t =>
                            {
                                // "متوسط زمن الإغلاق: "
                                t.Span(T("Report.TechCategories.Summary.AverageClosureHours") + ": ")
                                 .SemiBold();
                                t.Span($"{cat.AverageClosureHours.Value:0.##} " +
                                       T("Report.Common.HoursSuffix")); // "ساعة"
                            });
                        }

                        if (cat.AverageRequestsPerTechnician.HasValue)
                        {
                            c2.Item().Text(t =>
                            {
                                // "متوسط عدد الطلبات لكل فني: "
                                t.Span(T("Report.TechCategories.Summary.AverageRequestsPerTechnician") + ": ")
                                 .SemiBold();
                                t.Span($"{cat.AverageRequestsPerTechnician.Value:0.##}");
                            });
                        }
                    });

                // جدول الفنيين داخل الفئة
                if (cat.Technicians != null && cat.Technicians.Count > 0)
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  // #
                            columns.RelativeColumn();    // الاسم
                            columns.ConstantColumn(80);  // Assigned
                            columns.ConstantColumn(80);  // Completed
                            columns.ConstantColumn(80);  // Overdue
                            columns.ConstantColumn(110); // Avg Closure
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text(t => t.Span("#").SemiBold());
                            header.Cell().Text(t => t.Span(T("Report.TechCategories.Table.Header.Technician")).SemiBold());     // "الفني"
                            header.Cell().Text(t => t.Span(T("Report.TechCategories.Table.Header.Assigned")).SemiBold());       // "الطلبات"
                            header.Cell().Text(t => t.Span(T("Report.TechCategories.Table.Header.Completed")).SemiBold());      // "المكتملة"
                            header.Cell().Text(t => t.Span(T("Report.TechCategories.Table.Header.Overdue")).SemiBold());        // "المتأخرة"
                            header.Cell().Text(t => t.Span(T("Report.TechCategories.Table.Header.AvgClosureHours")).SemiBold()); // "متوسط زمن الإغلاق (س)"
                        });

                        int index = 1;
                        foreach (var t in cat.Technicians.OrderByDescending(x => x.AssignedCount))
                        {
                            table.Cell().Text(index.ToString());
                            table.Cell().Text(t.TechnicianName);
                            table.Cell().Text(t.AssignedCount.ToString());
                            table.Cell().Text(t.CompletedCount.ToString());
                            table.Cell().Text(t.OverdueCount.ToString());
                            table.Cell().Text(
                                t.AverageClosureHours.HasValue
                                    ? t.AverageClosureHours.Value.ToString("0.##")
                                    : "-");
                            index++;
                        }
                    });
                }
                else
                {
                    // "لا يوجد فنيون ضمن هذه الفئة في الفترة المحددة."
                    col.Item().Text(t =>
                    {
                        t.Span(T("Report.TechCategories.Category.NoTechnicians"))
                         .Italic();
                    });
                }
            });
        }
    }

}
