using Fixtroller.DAL.Data.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Fixtroller.BLL.Reports
{
    public class MaintenanceDepartmentReportDocument : IDocument
    {
        private readonly MaintenanceDepartmentReportDTO _model;

        public MaintenanceDepartmentReportDocument(MaintenanceDepartmentReportDTO model)
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
                            t.Span("تقرير قسم الصيانة ككل")
                             .FontSize(18)
                             .SemiBold();
                        });

                    // الفترة
                    col.Item()
                        .AlignCenter()
                        .Text(t =>
                        {
                            t.Span("الفترة: ").SemiBold();
                            t.Span(_model.FromUtc.ToString("yyyy-MM-dd"));
                            t.Span("  إلى  ");
                            t.Span(_model.ToUtc.ToString("yyyy-MM-dd"));
                        });
                });

                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    col.Item().Element(SummarySection);
                    col.Item().Element(CategoriesSection);
                    col.Item().Element(TopProblemTypesSection);
                    col.Item().Element(TopCategoriesSection);
                });

                page.Footer()
                    .AlignCenter()
                    .Text(txt =>
                    {
                        txt.Span("Fixtroller - Maintenance Department Report  ");
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
                        t.Span("الأرقام العامة للقسم")
                         .SemiBold()
                         .FontSize(14);
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("إجمالي الطلبات في الفترة: ").SemiBold();
                        t.Span(s.TotalRequests.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("عدد الطلبات الجديدة: ").SemiBold();
                        t.Span(s.NewRequests.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("عدد الطلبات المغلقة: ").SemiBold();
                        t.Span(s.ClosedRequests.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("عدد الطلبات المتبقية (المفتوحة): ").SemiBold();
                        t.Span(s.RemainingRequests.ToString());
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("عدد الطلبات المتأخرة (حسب SLA): ").SemiBold();
                        t.Span(s.OverdueRequests.ToString());
                    });

                    if (s.CompletionRate.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("نسبة الإنجاز: ").SemiBold();
                            t.Span($"{s.CompletionRate.Value:0.##}%");
                        });
                    }

                    if (s.OverdueRate.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("نسبة التأخير: ").SemiBold();
                            t.Span($"{s.OverdueRate.Value:0.##}%");
                        });
                    }

                    if (s.SlaComplianceRate.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("نسبة الالتزام بالـ SLA (من الطلبات المغلقة ذات SLA): ").SemiBold();
                            t.Span($"{s.SlaComplianceRate.Value:0.##}%");
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

                    col.Item().Text(t =>
                    {
                        t.Span("عدد الفنيين الكلي: ").SemiBold();
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
                    t.Span("لا توجد بيانات للفنيين أو الفئات ضمن هذه الفترة.")
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span("توزيع الفنيين والطلبات على الفئات (Categories)")
                     .SemiBold()
                     .FontSize(14);
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // #
                        columns.RelativeColumn();    // الفئة
                        columns.ConstantColumn(100); // عدد الفنيين
                        columns.ConstantColumn(100); // عدد الطلبات
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("#").SemiBold());
                        header.Cell().Text(t => t.Span("الفئة").SemiBold());
                        header.Cell().Text(t => t.Span("عدد الفنيين").SemiBold());
                        header.Cell().Text(t => t.Span("عدد الطلبات").SemiBold());
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
                    t.Span("لا توجد بيانات كافية عن أكثر أنواع المشاكل تكرارًا.")
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span("أكثر أنواع المشاكل تكرارًا (Top 3)")
                     .SemiBold()
                     .FontSize(14);
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // #
                        columns.RelativeColumn();    // نوع المشكلة
                        columns.ConstantColumn(80);  // العدد
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("#").SemiBold());
                        header.Cell().Text(t => t.Span("نوع المشكلة").SemiBold());
                        header.Cell().Text(t => t.Span("عدد الطلبات").SemiBold());
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
                    t.Span("لا توجد بيانات كافية عن أكثر الفئات (Categories) من حيث عدد الطلبات.")
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span("أكثر الفئات (Categories) من حيث عدد الطلبات (Top 3)")
                     .SemiBold()
                     .FontSize(14);
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // #
                        columns.RelativeColumn();    // الفئة
                        columns.ConstantColumn(100); // عدد الطلبات
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(t => t.Span("#").SemiBold());
                        header.Cell().Text(t => t.Span("الفئة").SemiBold());
                        header.Cell().Text(t => t.Span("عدد الطلبات").SemiBold());
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
