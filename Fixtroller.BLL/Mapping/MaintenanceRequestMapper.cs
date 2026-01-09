using Fixtroller.BLL.Helpers;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fixtroller.BLL.Mapping
{
    public static class MaintenanceRequestMapper
    {
        public static MaintenanceRequest ToEntity(
            MaintenanceRequestRequestDTO request,
            string ownerUserId,
            string createdByUserId)
        {
            // parse lat/lng (strings) safely with invariant culture
            decimal? lat = null;
            decimal? lng = null;

            if (TryParseInvariant(request.Latitude, out var latVal) &&
                TryParseInvariant(request.Longitude, out var lngVal))
            {
                lat = latVal;
                lng = lngVal;
            }

            return new MaintenanceRequest
            {
                Title = request.Title?.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim(),

                Priority = request.Priority,

                Address = string.IsNullOrWhiteSpace(request.Address)
                    ? null
                    : request.Address.Trim(),

                Latitude = lat,
                Longitude = lng,

                ProblemTypeId = request.ProblemTypeId,
                CaseType = CaseType.Submitted,
                OwnerUserId = ownerUserId,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };
        }



        // 1) النسخة الافتراضية (تبني URL نسبي)
        public static MaintenanceRequestResponseDTO ToResponse(MaintenanceRequest e, string role)
            => ToResponse(e, role, fileName => $"/Images/{fileName}");

        // 2) نسخة تقبل مولّد روابط مخصّص (اختياري)
        public static MaintenanceRequestResponseDTO ToResponse(
            MaintenanceRequest e,
            string role,
            Func<string, string> urlBuilder,
            string language = "ar",
            bool isOwner = false,
            bool includeOwnerDetails = false
            )
        {
            var effectiveCase =
                (isOwner
                 && role.Equals("Employee", StringComparison.OrdinalIgnoreCase)
                 && e.CaseType == CaseType.ManagerReview)
                    ? CaseType.Processing
                    : e.CaseType;

            var ptName = e.ProblemType?.Translations?
                 .OrderBy(t =>
                     t.Language == language ? 0 :
                     t.Language == "ar" ? 1 : 2)
                 .Select(t => t.Name)
                 .FirstOrDefault();

            var owner = e.OwnerUser;

            var dto = new MaintenanceRequestResponseDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Priority = e.Priority,
                PriorityName = GetPriorityName(e.Priority, language),
                CaseType = GetCaseTypeName(effectiveCase, language),

                ProblemTypeId = e.ProblemTypeId,
                ProblemTypeName = ptName,
                Address = e.Address,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                OwnerUserId = e.OwnerUserId,
                OwnerName = owner != null ? owner.GetDisplayName(language) : null,
                CreatedByUserId = e.CreatedByUserId,
                IsCreatedByOwner = string.Equals(e.OwnerUserId, e.CreatedByUserId, StringComparison.Ordinal),

                CreatedAt = e.CreatedAt,
                LastModifiedAt = e.UpdatedAt,
                ClosedAtUtc = e.ClosedAtUtc
            };

            if (owner != null && includeOwnerDetails)
            {
                dto.OwnerDepartment = owner.Department;

                if (!string.IsNullOrWhiteSpace(owner.ProfileImagePath))
                {
                    dto.OwnerProfileImageUrl = urlBuilder(owner.ProfileImagePath);
                }
            }


            if (e.Images is not null && e.Images.Count > 0)
            {
                dto.Images = e.Images
                    .OrderByDescending(i => i.IsPrimary)
                    .Select(i => new MaintenanceRequestImageDTO
                    {
                        Id = i.Id,
                        Url = urlBuilder(i.FileName),
                        IsPrimary = i.IsPrimary,
                        IsStaffAttachment = i.Source == MaintenanceRequestImageSource.StaffAttachment
                    })
                    .ToList();
            }
            if (e.Notes is not null && e.Notes.Count > 0)
            {
                dto.Notes = e.Notes
                 .OrderByDescending(n => n.CreatedAt)
                 .Select(n => new MaintenanceNoteDTO
                 {
                     Id = n.Id,
                     Text = n.Text,
                     Type = GetNoteTypeName(n.Type, language),
                     Author = GetNoteAuthorName(n.Author, language),
                     CreatedByUserId = n.CreatedByUserId,
                     CreatedByName = n.CreatedByUser != null
                         ? n.CreatedByUser.GetDisplayName(language)   
                         : n.CreatedByUserId,
                     CreatedAt = n.CreatedAt
                 })
                 .ToList();
            }

            dto.AssignedTechnicians = (e.Technicians ?? Enumerable.Empty<MaintenanceRequestTechnician>())
                .Where(t => t.UnassignedAtUtc == null)
                .OrderByDescending(t => t.AssignedAtUtc)
                .Select(t => new AssignedTechnicianDTO
                {
                    UserId = t.TechnicianUserId,
                    AssignedAtUtc = t.AssignedAtUtc.Kind == DateTimeKind.Utc
                        ? t.AssignedAtUtc
                        : DateTime.SpecifyKind(t.AssignedAtUtc, DateTimeKind.Utc),
                    ExpectedDuration = t.ExpectedDuration
                })
                .ToList();



            return dto;
        }
        public static MaintenanceNote ToNote(string text, NoteType type, NoteAuthor author, string createdByUserId, int requestId)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Note text cannot be empty.", nameof(text));

            return new MaintenanceNote
            {
                MaintenanceRequestId = requestId,
                Text = text.Trim(),
                Type = type,
                Author = author,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };
        }
        public static string GetCaseTypeName(CaseType c, string lang)
        {
            var isAr = string.Equals(lang, "ar", StringComparison.OrdinalIgnoreCase);

            if (isAr)
            {
                return c switch
                {
                    CaseType.Submitted => "تم التقديم",
                    CaseType.ManagerReview => "مراجعة المدير",
                    CaseType.Processing => "قيد المعالجة",
                    CaseType.ResourcesNeeded => "بحاجة موارد",
                    CaseType.Processed => "تمت المعالجة",
                    CaseType.Modified => "تم التعديل",
                    CaseType.Reopened => "أعيد فتحه",
                    CaseType.Completed => "مكتمل",
                    CaseType.Cancelled => "ملغي",
                    _ => c.ToString()
                };
            }

            // English (default)
            return c switch
            {
                CaseType.Submitted => "Submitted",
                CaseType.ManagerReview => "Manager Review",
                CaseType.Processing => "Processing",
                CaseType.ResourcesNeeded => "Resources Needed",
                CaseType.Processed => "Processed",
                CaseType.Modified => "Modified",
                CaseType.Reopened => "Reopened",
                CaseType.Completed => "Completed",
                CaseType.Cancelled => "Cancelled",
                _ => c.ToString()
            };
        }

        public static MaintenanceRequestListMineDTO ToMineListItem(
          MaintenanceRequest e, string role, bool isOwner, string language = "ar", string? problemTypeName = null)
        {

            var effectiveCase =
                (isOwner
                 && role.Equals("Employee", StringComparison.OrdinalIgnoreCase)
                 && e.CaseType == CaseType.ManagerReview)
                    ? CaseType.Processing
                    : e.CaseType;

            return new MaintenanceRequestListMineDTO
            {
                Id = e.Id,
                Title = e.Title,
                CaseType = GetCaseTypeName(effectiveCase, language),
                ProblemTypeName = problemTypeName,
                Priority = GetPriorityName(e.Priority, language),
                CreatedAt = e.CreatedAt,
                LastModifiedAt = e.UpdatedAt
            };
        }


        public static MaintenanceRequestListMineDTO ToMineListItem(
            MaintenanceRequest e, string language = "ar")
            => ToMineListItem(e, role: "Employee", isOwner: false, language);


        public static MaintenanceRequestListAllDTO ToAllListItem(
            MaintenanceRequest e,
            string? problemTypeName,
            string language = "ar",
            string? technicianUserId = null,
            string? technicianName = null)
        {
            return new MaintenanceRequestListAllDTO
            {
                Id = e.Id,
                Title = e.Title,
                CaseType = GetCaseTypeName(e.CaseType, language),
                Priority = GetPriorityName(e.Priority, language),
                CreatedAt = e.CreatedAt,
                ProblemTypeName = problemTypeName,
                AssignedTechnicianUserId = technicianUserId,
                AssignedTechnicianName = technicianName
            };
        }


        public static TechnicianTaskCardDTO ToTechnicianCard(
    MaintenanceRequest e,
    string language = "ar",
    string? problemTypeName = null)
        {
            return new TechnicianTaskCardDTO
            {
                Id = e.Id,
                Title = e.Title ?? string.Empty,
                Description = e.Description,
                Priority = GetPriorityName(e.Priority, language),
                CaseType = GetCaseTypeName(e.CaseType, language),
                ProblemTypeName = problemTypeName,
                CreatedAt = e.CreatedAt
            };
        }

        private static string GetNoteTypeName(NoteType type, string language = "ar")
        {
            // لو حاب تحوّلها إلى Resource لاحقًا، سهّلها هنا
            return language.Equals("ar", StringComparison.OrdinalIgnoreCase) ? type switch
            {
                NoteType.General => "ملاحظة",
                NoteType.ReopenReason => "سبب إعادة الفتح",
                NoteType.HelpRequest => "طلب مساعدة",
                _ => "ملاحظة"
            } : type switch
            {
                NoteType.General => "Note",
                NoteType.ReopenReason => "Reopen reason",
                NoteType.HelpRequest => "Help request",
                _ => "Note"
            };
        }
        private static string GetNoteAuthorName(NoteAuthor author, string language = "ar") =>
        language.Equals("ar", StringComparison.OrdinalIgnoreCase) ? author switch
        {
            NoteAuthor.Owner => "مالك الطلب",
            NoteAuthor.Technician => "الفني",
            NoteAuthor.Manager => "المدير",
            NoteAuthor.Admin => "المدير (أدمن)",
        } : author switch
        {
            NoteAuthor.Owner => "Owner",
            NoteAuthor.Technician => "Technician",
            NoteAuthor.Manager => "Manager",
            NoteAuthor.Admin => "Admin",
        };

        public static string GetPriorityName(Priority p, string lang)
        {
            var isAr = string.Equals(lang, "ar", StringComparison.OrdinalIgnoreCase);

            if (isAr)
            {
                return p switch
                {
                    Priority.High => "عالية",
                    Priority.Medium => "متوسطة",
                    Priority.Low => "منخفضة",
                    _ => p.ToString()
                };
            }

            // English 
            return p switch
            {
                Priority.High => "High",
                Priority.Medium => "Medium",
                Priority.Low => "Low",
                _ => p.ToString()
            };
        }
        private static bool TryParseInvariant(string? s, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            var normalized = s.Trim().Replace("،", ",").Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

    }
}
