using Fixtroller.DAL.Data.DTOs.Reports.Responses;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using Fixtroller.BLL.Reports;   // ✅ جديد

namespace Fixtroller.BLL.Reports.ReportsTypes
{
    public class SingleRequestReportDocument : IDocument
    {
        private readonly SingleRequestReportDTO _model;
        private readonly IReportsTextBuilder _text;
        private readonly string _language;

        public SingleRequestReportDocument(SingleRequestReportDTO model, IReportsTextBuilder text, string language)
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

                // ✅ هيدر موحّد: شعار + "تقرير طلب صيانة رقم X" + عنوان الطلب
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
                        col.Item().Element(OwnerSection);
                        col.Item().Element(TimingSection);
                        col.Item().Element(TechniciansSection);
                        col.Item().Element(NotesSection);
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
            // "تقرير طلب صيانة رقم {0}"
            var title = T("Report.SingleRequest.Header.Title", _model.RequestId);

            // العنوان الفرعي = عنوان الطلب نفسه من الداتا
            var subtitle = _model.Title;

            // يرسم الشعار + العنوان + العنوان الفرعي + خط تحتهم
            ReportBranding.RenderHeader(container, title, subtitle);
        }
        // =====================================================

        void SummarySection(IContainer container)
        {
            container.BorderBottom(1).PaddingBottom(5).Column(col =>
            {
                col.Spacing(4);

                // "معلومات الطلب"
                col.Item().Text(T("Report.SingleRequest.Section.Summary"))
                    .SemiBold().FontSize(14);

                col.Item().Text(text =>
                {
                    // "نوع المشكلة: "
                    text.Span(T("Report.SingleRequest.Label.ProblemType") + ": ")
                        .SemiBold();
                    text.Span(_model.ProblemTypeName);
                });

                col.Item().Text(text =>
                {
                    // "الأولوية: "
                    text.Span(T("Report.SingleRequest.Label.Priority") + ": ")
                        .SemiBold();
                    text.Span(_model.PriorityName);
                });

                col.Item().Text(text =>
                {
                    // "الحالة الحالية: "
                    text.Span(T("Report.SingleRequest.Label.CaseType") + ": ")
                        .SemiBold();
                    text.Span(_model.CaseTypeName);
                });

                if (!string.IsNullOrWhiteSpace(_model.Description))
                {
                    col.Item().Text(text =>
                    {
                        // "وصف المشكلة: "
                        text.Span(T("Report.SingleRequest.Label.Description") + ": ")
                            .SemiBold();
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

                // "صاحب الطلب"
                col.Item().Text(T("Report.SingleRequest.Section.Owner"))
                    .SemiBold().FontSize(14);

                col.Item().Text(text =>
                {
                    // "الاسم: "
                    text.Span(T("Report.SingleRequest.Label.OwnerName") + ": ")
                        .SemiBold();
                    text.Span(_model.OwnerFullName);
                });

                if (!string.IsNullOrWhiteSpace(_model.OwnerDepartment))
                {
                    col.Item().Text(text =>
                    {
                        // "القسم: "
                        text.Span(T("Report.SingleRequest.Label.OwnerDepartment") + ": ")
                            .SemiBold();
                        text.Span(_model.OwnerDepartment!);
                    });
                }

                if (!string.IsNullOrWhiteSpace(_model.OwnerLocation))
                {
                    col.Item().Text(text =>
                    {
                        // "موقع الموظف: "
                        text.Span(T("Report.SingleRequest.Label.OwnerLocation") + ": ")
                            .SemiBold();
                        text.Span(_model.OwnerLocation!);
                    });
                }

                if (!string.IsNullOrWhiteSpace(_model.RequestAddress))
                {
                    col.Item().Text(text =>
                    {
                        // "موقع المشكلة (مبنى / غرفة): "
                        text.Span(T("Report.SingleRequest.Label.RequestAddress") + ": ")
                            .SemiBold();
                        text.Span(_model.RequestAddress!);
                    });
                }

                col.Item().Text(text =>
                {
                    // "مُنشئ الطلب: "
                    text.Span(T("Report.SingleRequest.Label.CreatedBy") + ": ")
                        .SemiBold();
                    text.Span(_model.CreatedByFullName);
                    text.Span("  (");

                    var createdByText = _model.IsCreatedByOwner
                        ? T("Report.SingleRequest.CreatedBy.SameAsOwner")
                        : T("Report.SingleRequest.CreatedBy.OtherUser");

                    text.Span(createdByText);
                    text.Span(")");
                });
            });
        }

        void TimingSection(IContainer container)
        {
            container.BorderBottom(1).PaddingBottom(5).Column(col =>
            {
                col.Spacing(4);

                // "الأوقات والـ SLA"
                col.Item().Text(T("Report.SingleRequest.Section.Timing"))
                    .SemiBold().FontSize(14);

                col.Item().Text(text =>
                {
                    // "تاريخ إنشاء الطلب: "
                    text.Span(T("Report.SingleRequest.Label.CreatedAt") + ": ")
                        .SemiBold();
                    text.Span(_model.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm"));
                });

                if (_model.FirstAssignedAtUtc is not null)
                {
                    col.Item().Text(text =>
                    {
                        // "تاريخ أول تعيين لفني: "
                        text.Span(T("Report.SingleRequest.Label.FirstAssignedAt") + ": ")
                            .SemiBold();
                        text.Span(_model.FirstAssignedAtUtc.Value.ToString("yyyy-MM-dd HH:mm"));
                    });
                }

                if (_model.FirstWorkStartedAtUtc is not null)
                {
                    col.Item().Text(text =>
                    {
                        // "تاريخ أول بدء عمل فعلي: "
                        text.Span(T("Report.SingleRequest.Label.FirstWorkStartedAt") + ": ")
                            .SemiBold();
                        text.Span(_model.FirstWorkStartedAtUtc.Value.ToString("yyyy-MM-dd HH:mm"));
                    });
                }

                if (_model.ClosedAtUtc is not null)
                {
                    col.Item().Text(text =>
                    {
                        // "تاريخ الإغلاق: "
                        text.Span(T("Report.SingleRequest.Label.ClosedAt") + ": ")
                            .SemiBold();
                        text.Span(_model.ClosedAtUtc.Value.ToString("yyyy-MM-dd HH:mm"));
                    });
                }

                if (_model.ExpectedDurationHours is not null)
                {
                    var value = T("Report.SingleRequest.Duration.Expected.Format",
                        _model.ExpectedDurationHours.Value);

                    col.Item().Text(text =>
                    {
                        // "المدة المتوقعة (SLA): "
                        text.Span(T("Report.SingleRequest.Label.ExpectedDuration") + ": ")
                            .SemiBold();
                        text.Span(value);
                    });
                }

                if (_model.ActualDurationHours is not null)
                {
                    var value = T("Report.SingleRequest.Duration.Actual.Format",
                        _model.ActualDurationHours.Value);

                    col.Item().Text(text =>
                    {
                        // "المدة الفعلية للإغلاق: "
                        text.Span(T("Report.SingleRequest.Label.ActualDuration") + ": ")
                            .SemiBold();
                        text.Span(value);
                    });
                }

                if (_model.IsWithinSla is not null)
                {
                    var statusKey = _model.IsWithinSla.Value
                        ? "Report.SingleRequest.SlaStatus.Within"
                        : "Report.SingleRequest.SlaStatus.Late";

                    var status = T(statusKey);

                    col.Item().Text(text =>
                    {
                        // "حالة الالتزام بالـ SLA: "
                        text.Span(T("Report.SingleRequest.Label.SlaStatus") + ": ")
                            .SemiBold();
                        text.Span(status);
                    });
                }
            });
        }

        void TechniciansSection(IContainer container)
        {
            if (_model.Technicians == null || _model.Technicians.Count == 0)
            {
                container.Text(T("Report.SingleRequest.Message.NoTechnicians"))
                         .Italic();
                return;
            }

            container.BorderBottom(1).PaddingBottom(5).Column(col =>
            {
                col.Spacing(4);

                // "الفنيون المشاركون"
                col.Item().Text(T("Report.SingleRequest.Section.Technicians"))
                    .SemiBold().FontSize(14);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(120); // الاسم
                        columns.RelativeColumn();    // الفئة
                        columns.ConstantColumn(80);  // تعيين
                        columns.ConstantColumn(80);  // بدء
                        columns.ConstantColumn(80);  // انتهاء
                        columns.ConstantColumn(60);  // ساعات عمل
                        columns.ConstantColumn(50);  // SLA (س)
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text(T("Report.SingleRequest.Technicians.Header.Name")).SemiBold();
                        header.Cell().Text(T("Report.SingleRequest.Technicians.Header.Category")).SemiBold();
                        header.Cell().Text(T("Report.SingleRequest.Technicians.Header.AssignedAt")).SemiBold();
                        header.Cell().Text(T("Report.SingleRequest.Technicians.Header.StartAt")).SemiBold();
                        header.Cell().Text(T("Report.SingleRequest.Technicians.Header.EndAt")).SemiBold();
                        header.Cell().Text(T("Report.SingleRequest.Technicians.Header.WorkHours")).SemiBold();
                        header.Cell().Text(T("Report.SingleRequest.Technicians.Header.SlaHours")).SemiBold();
                    });

                    foreach (var t in _model.Technicians.OrderBy(x => x.AssignedAtUtc))
                    {
                        var workHoursText = t.TotalWorkHours.HasValue && t.TotalWorkHours.Value > 0
                            ? t.TotalWorkHours.Value.ToString("0.##")
                            : "-";

                        var expectedHoursText = t.ExpectedDurationHours.HasValue && t.ExpectedDurationHours.Value > 0
                            ? t.ExpectedDurationHours.Value.ToString("0.##")
                            : "-";

                        table.Cell().Text(t.TechnicianName);
                        table.Cell().Text(t.TechnicianCategory ?? "-");
                        table.Cell().Text(t.AssignedAtUtc.ToString("MM-dd HH:mm"));
                        table.Cell().Text(t.FirstWorkStartedAtUtc?.ToString("MM-dd HH:mm") ?? "-");
                        table.Cell().Text(t.LastWorkStoppedAtUtc?.ToString("MM-dd HH:mm") ?? "-");
                        table.Cell().Text(workHoursText);
                        table.Cell().Text(expectedHoursText);
                    }
                });
            });
        }

        void NotesSection(IContainer container)
        {
            if (_model.Notes == null || _model.Notes.Count == 0)
            {
                container.Text(T("Report.SingleRequest.Message.NoNotes"))
                         .Italic();
                return;
            }

            container.Column(col =>
            {
                col.Spacing(4);

                // "الملاحظات"
                col.Item().Text(T("Report.SingleRequest.Section.Notes"))
                    .SemiBold().FontSize(14);

                foreach (var n in _model.Notes.OrderByDescending(x => x.CreatedAt))
                {
                    col.Item().Border(1).Padding(5).Column(c2 =>
                    {
                        var header = T("Report.SingleRequest.Notes.HeaderFormat",
                            n.CreatedAt, n.CreatedByName);

                        c2.Item().Text(header)
                            .FontSize(10).SemiBold();

                        c2.Item().Text(n.Text);
                    });
                }
            });
        }
    }
}
