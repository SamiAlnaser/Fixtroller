using Fixtroller.DAL.Data.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Fixtroller.BLL.Reports
{
    public class SingleRequestReportDocument : IDocument
    {
        private readonly SingleRequestReportDTO _model;

        public SingleRequestReportDocument(SingleRequestReportDTO model)
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
                    col.Item().Text($"تقرير طلب صيانة رقم {_model.RequestId}")
                        .FontSize(20).SemiBold().AlignCenter();

                    col.Item().Text(_model.Title)
                        .FontSize(12)
                        .AlignCenter();
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Element(SummarySection);
                    col.Item().Element(OwnerSection);
                    col.Item().Element(TimingSection);
                    col.Item().Element(TechniciansSection);
                    col.Item().Element(NotesSection);
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Fixtroller - Maintenance Report  ");
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
            container.BorderBottom(1).PaddingBottom(5).Column(col =>
            {
                col.Spacing(4);
                col.Item().Text("معلومات الطلب").SemiBold().FontSize(14);

                col.Item().Text(text =>
                {
                    text.Span("نوع المشكلة: ").SemiBold();
                    text.Span(_model.ProblemTypeName);
                });

                col.Item().Text(text =>
                {
                    text.Span("الأولوية: ").SemiBold();
                    text.Span(_model.PriorityName);
                });

                col.Item().Text(text =>
                {
                    text.Span("الحالة الحالية: ").SemiBold();
                    text.Span(_model.CaseTypeName);
                });

                if (!string.IsNullOrWhiteSpace(_model.Description))
                {
                    col.Item().Text(text =>
                    {
                        text.Span("وصف المشكلة: ").SemiBold();
                        text.Span(_model.Description);
                    });
                }
            });
        }

        void OwnerSection(IContainer container)
        {
            container.BorderBottom(1).PaddingBottom(5).Column(col =>
            {
                col.Spacing(4);
                col.Item().Text("صاحب الطلب").SemiBold().FontSize(14);

                col.Item().Text(text =>
                {
                    text.Span("الاسم: ").SemiBold();
                    text.Span(_model.OwnerFullName);
                });

                if (!string.IsNullOrWhiteSpace(_model.OwnerDepartment))
                {
                    col.Item().Text(text =>
                    {
                        text.Span("القسم: ").SemiBold();
                        text.Span(_model.OwnerDepartment!);
                    });
                }

                if (!string.IsNullOrWhiteSpace(_model.OwnerLocation))
                {
                    col.Item().Text(text =>
                    {
                        text.Span("موقع الموظف: ").SemiBold();
                        text.Span(_model.OwnerLocation!);
                    });
                }

                if (!string.IsNullOrWhiteSpace(_model.RequestAddress))
                {
                    col.Item().Text(text =>
                    {
                        text.Span("موقع المشكلة (مبنى / غرفة): ").SemiBold();
                        text.Span(_model.RequestAddress!);
                    });
                }

                col.Item().Text(text =>
                {
                    text.Span("مُنشئ الطلب: ").SemiBold();
                    text.Span(_model.CreatedByFullName);
                    text.Span("  (");
                    text.Span(_model.IsCreatedByOwner ? "نفس صاحب الطلب" : "مستخدم آخر (فني / مدير)");
                    text.Span(")");
                });
            });
        }

        void TimingSection(IContainer container)
        {
            container.BorderBottom(1).PaddingBottom(5).Column(col =>
            {
                col.Spacing(4);
                col.Item().Text("الأوقات والـ SLA").SemiBold().FontSize(14);

                col.Item().Text(text =>
                {
                    text.Span("تاريخ إنشاء الطلب: ").SemiBold();
                    text.Span(_model.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm"));
                });

                if (_model.FirstAssignedAtUtc is not null)
                {
                    col.Item().Text(text =>
                    {
                        text.Span("تاريخ أول تعيين لفني: ").SemiBold();
                        text.Span(_model.FirstAssignedAtUtc.Value.ToString("yyyy-MM-dd HH:mm"));
                    });
                }

                if (_model.FirstWorkStartedAtUtc is not null)
                {
                    col.Item().Text(text =>
                    {
                        text.Span("تاريخ أول بدء عمل فعلي: ").SemiBold();
                        text.Span(_model.FirstWorkStartedAtUtc.Value.ToString("yyyy-MM-dd HH:mm"));
                    });
                }

                if (_model.ClosedAtUtc is not null)
                {
                    col.Item().Text(text =>
                    {
                        text.Span("تاريخ الإغلاق: ").SemiBold();
                        text.Span(_model.ClosedAtUtc.Value.ToString("yyyy-MM-dd HH:mm"));
                    });
                }

                if (_model.ExpectedDurationHours is not null)
                {
                    col.Item().Text(text =>
                    {
                        text.Span("المدة المتوقعة (SLA): ").SemiBold();
                        text.Span($"{_model.ExpectedDurationHours:0.##} ساعة");
                    });
                }

                if (_model.ActualDurationHours is not null)
                {
                    col.Item().Text(text =>
                    {
                        text.Span("المدة الفعلية للإغلاق: ").SemiBold();
                        text.Span($"{_model.ActualDurationHours:0.##} ساعة");
                    });
                }

                if (_model.IsWithinSla is not null)
                {
                    var status = _model.IsWithinSla.Value ? "داخل الـ SLA" : "متأخر عن الـ SLA";
                    col.Item().Text(text =>
                    {
                        text.Span("حالة الالتزام بالـ SLA: ").SemiBold();
                        text.Span(status);
                    });
                }
            });
        }

        void TechniciansSection(IContainer container)
        {
            if (_model.Technicians == null || _model.Technicians.Count == 0)
            {
                container.Text("لا يوجد فنيون مُعينون على هذا الطلب.")
                         .Italic();
                return;
            }

            container.BorderBottom(1).PaddingBottom(5).Column(col =>
            {
                col.Spacing(4);
                col.Item().Text("الفنيون المشاركون").SemiBold().FontSize(14);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(120); // الاسم - قللناه شوي
                        columns.RelativeColumn();    // الفئة - ياخذ الباقي
                        columns.ConstantColumn(80);  // تاريخ التعيين
                        columns.ConstantColumn(80);  // بدء العمل
                        columns.ConstantColumn(80);  // انتهاء العمل
                        columns.ConstantColumn(60);  // مدة العمل
                        columns.ConstantColumn(50);  // SLA
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Text("الفني").SemiBold();
                        header.Cell().Text("الفئة").SemiBold();
                        header.Cell().Text("تعيين").SemiBold();
                        header.Cell().Text("بدء").SemiBold();
                        header.Cell().Text("انتهاء").SemiBold();
                        header.Cell().Text("دقائق عمل").SemiBold();
                        header.Cell().Text("SLA (س)").SemiBold();
                    });

                    foreach (var t in _model.Technicians.OrderBy(x => x.AssignedAtUtc))
                    {
                        table.Cell().Text(t.TechnicianName);
                        table.Cell().Text(t.TechnicianCategory ?? "-");
                        table.Cell().Text(t.AssignedAtUtc.ToString("MM-dd HH:mm"));
                        table.Cell().Text(t.FirstWorkStartedAtUtc?.ToString("MM-dd HH:mm") ?? "-");
                        table.Cell().Text(t.LastWorkStoppedAtUtc?.ToString("MM-dd HH:mm") ?? "-");
                        table.Cell().Text(t.TotalWorkMinutes > 0 ? t.TotalWorkMinutes.ToString("0") : "-");
                        table.Cell().Text(t.ExpectedDurationHours?.ToString() ?? "-");
                    }
                });
            });
        }

        void NotesSection(IContainer container)
        {
            if (_model.Notes == null || _model.Notes.Count == 0)
            {
                container.Text("لا توجد ملاحظات على هذا الطلب.")
                         .Italic();
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);
                col.Item().Text("الملاحظات").SemiBold().FontSize(14);

                foreach (var n in _model.Notes.OrderByDescending(x => x.CreatedAt))
                {
                    col.Item().Border(1).Padding(5).Column(c2 =>
                    {
                        c2.Item().Text($"{n.CreatedAt:yyyy-MM-dd HH:mm} - {n.CreatedByName}")
                            .FontSize(10).SemiBold();
                        c2.Item().Text(n.Text);
                    });
                }
            });
        }
    }
}
