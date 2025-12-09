using Fixtroller.DAL.Data.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Fixtroller.BLL.Reports
{
    public class KpiRequestsReportDocument : IDocument
    {
        private readonly KpiRequestsReportDTO _model;

        public KpiRequestsReportDocument(KpiRequestsReportDTO model)
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
                            t.Span("تقرير الأرقام العامة (KPI)")
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

                    // نوع المشكلة لو فيه فلتر
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
                    col.Item().Element(TopProblemTypesSection);
                    col.Item().Element(TopDepartmentsSection);
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Fixtroller - KPI Report  ");
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
                        t.Span("الأرقام العامة")
                         .FontSize(14)
                         .SemiBold();
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("إجمالي الطلبات في الفترة: ").SemiBold();
                        text.Span(s.TotalRequests.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("عدد الطلبات الجديدة: ").SemiBold();
                        text.Span(s.NewRequests.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("عدد الطلبات المغلقة: ").SemiBold();
                        text.Span(s.ClosedRequests.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("عدد الطلبات المتبقية (المفتوحة): ").SemiBold();
                        text.Span(s.RemainingRequests.ToString());
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("عدد الطلبات المتأخرة (حسب SLA): ").SemiBold();
                        text.Span(s.OverdueRequests.ToString());
                    });

                    if (s.CompletionRate.HasValue)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("نسبة الإنجاز: ").SemiBold();
                            text.Span($"{s.CompletionRate.Value:0.##}%");
                        });
                    }

                    if (s.OverdueRate.HasValue)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("نسبة التأخير: ").SemiBold();
                            text.Span($"{s.OverdueRate.Value:0.##}%");
                        });
                    }

                    if (s.SlaComplianceRate.HasValue)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("نسبة الالتزام بالـ SLA (من الطلبات المغلقة ذات SLA): ").SemiBold();
                            text.Span($"{s.SlaComplianceRate.Value:0.##}%");
                        });
                    }

                    if (s.AverageClosureHours.HasValue)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("متوسط زمن الإغلاق: ").SemiBold();
                            text.Span($"{s.AverageClosureHours.Value:0.##} ساعة");
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
                    t.Span("لا توجد بيانات كافية لحساب أكثر أنواع المشاكل تكرارًا.")
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span("أكثر أنواع المشاكل تكرارًا")
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
                        header.Cell().Text(t => t.Span("نوع المشكلة").SemiBold());
                        header.Cell().Text(t => t.Span("العدد").SemiBold());
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
                    t.Span("لا توجد بيانات كافية لحساب أكثر الأقسام تكرارًا.")
                     .Italic();
                });
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(t =>
                {
                    t.Span("أكثر الأقسام تكرارًا")
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
                        header.Cell().Text(t => t.Span("القسم").SemiBold());
                        header.Cell().Text(t => t.Span("العدد").SemiBold());
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
