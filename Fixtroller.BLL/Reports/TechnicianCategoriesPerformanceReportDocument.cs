using Fixtroller.DAL.Data.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Fixtroller.BLL.Reports
{
    public class TechnicianCategoriesPerformanceReportDocument : IDocument
    {
        private readonly TechnicianCategoriesPerformanceReportDTO _model;

        public TechnicianCategoriesPerformanceReportDocument(TechnicianCategoriesPerformanceReportDTO model)
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
                            t.Span("تقرير الفنيين حسب الفئة (Category)")
                             .FontSize(18)
                             .SemiBold();
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
                    col.Spacing(15);

                    if (_model.Categories == null || _model.Categories.Count == 0)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("لا توجد بيانات للفنيين ضمن الفترة المحددة.")
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
                        txt.Span("Fixtroller - Technicians by Category Report  ");
                        txt.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                        txt.Span("  |  صفحة ");
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

                // عنوان الفئة
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
                            t.Span("عدد الفنيين: ").SemiBold();
                            t.Span(cat.TechniciansCount.ToString());
                        });

                        c2.Item().Text(t =>
                        {
                            t.Span("عدد الطلبات في الفترة: ").SemiBold();
                            t.Span(cat.TotalAssigned.ToString());
                        });

                        c2.Item().Text(t =>
                        {
                            t.Span("عدد الطلبات المكتملة: ").SemiBold();
                            t.Span(cat.TotalCompleted.ToString());
                        });

                        c2.Item().Text(t =>
                        {
                            t.Span("عدد الطلبات المتأخرة: ").SemiBold();
                            t.Span(cat.TotalOverdue.ToString());
                        });

                        if (cat.CompletionRate.HasValue)
                        {
                            c2.Item().Text(t =>
                            {
                                t.Span("نسبة الإنجاز: ").SemiBold();
                                t.Span($"{cat.CompletionRate.Value:0.##}%");
                            });
                        }

                        if (cat.OverdueRate.HasValue)
                        {
                            c2.Item().Text(t =>
                            {
                                t.Span("نسبة التأخير: ").SemiBold();
                                t.Span($"{cat.OverdueRate.Value:0.##}%");
                            });
                        }

                        if (cat.AverageClosureHours.HasValue)
                        {
                            c2.Item().Text(t =>
                            {
                                t.Span("متوسط زمن الإغلاق: ").SemiBold();
                                t.Span($"{cat.AverageClosureHours.Value:0.##} ساعة");
                            });
                        }

                        if (cat.AverageRequestsPerTechnician.HasValue)
                        {
                            c2.Item().Text(t =>
                            {
                                t.Span("متوسط عدد الطلبات لكل فني: ").SemiBold();
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
                            header.Cell().Text(t => t.Span("الفني").SemiBold());
                            header.Cell().Text(t => t.Span("الطلبات").SemiBold());
                            header.Cell().Text(t => t.Span("المكتملة").SemiBold());
                            header.Cell().Text(t => t.Span("المتأخرة").SemiBold());
                            header.Cell().Text(t => t.Span("متوسط زمن الإغلاق (س)").SemiBold());
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
                    col.Item().Text(t =>
                    {
                        t.Span("لا يوجد فنيون ضمن هذه الفئة في الفترة المحددة.")
                         .Italic();
                    });
                }
            });
        }
    }
}
