using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Mapping
{
    public static class MaintenanceRequestMapper
    {
        public static MaintenanceRequest ToEntity(
            MaintenanceRequestRequestDTO request,
            string createdByUserId)
        {
            return new MaintenanceRequest
            {
                Title = request.Title?.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Priority = request.Priority,
                Address = request.Address?.Trim(),
                ProblemTypeId = request.ProblemTypeId,
                CaseType = CaseType.Submitted,
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
            bool isOwner = false
            )
        {
            var effectiveCase =
                (isOwner
                 && role.Equals("Employee", StringComparison.OrdinalIgnoreCase)
                 && e.CaseType == CaseType.ManagerReview)
                    ? CaseType.Processing
                    : e.CaseType;

            var dto = new MaintenanceRequestResponseDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Priority = e.Priority,
                CaseType = GetCaseTypeName(effectiveCase, language),
                Address = e.Address,
                CreatedByUserId = e.CreatedByUserId,
                CreatedAt = e.CreatedAt,
                LastModifiedAt = e.UpdatedAt
            };

            if (e.Images is not null && e.Images.Count > 0)
            {
                dto.Images = e.Images
                    .OrderByDescending(i => i.IsPrimary)
                    .Select(i => new MaintenanceRequestImageDTO
                    {
                        Id = i.Id,
                        Url = urlBuilder(i.FileName),
                        IsPrimary = i.IsPrimary
                    })
                    .ToList();
            }
            dto.AssignedTechnician = e.AssignedTechnician == null
                ? null
                : TechnicianMappings.ToTechnicianResponse(e.AssignedTechnician, language);

            

            return dto;
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

    }
}
