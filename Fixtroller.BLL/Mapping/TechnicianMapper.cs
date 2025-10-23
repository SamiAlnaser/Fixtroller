using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Mapping
{
    public static class TechnicianMappings
    {
        public static TechnicianResponseDTO ToTechnicianResponse(ApplicationUser user, string language = "ar")
        {
            if (user == null) return null;

            string? catName =
                user.TechnicianCategory?.Translations?
                    .FirstOrDefault(t => t.Language == language)?.Name
                ?? user.TechnicianCategory?.Translations?.FirstOrDefault()?.Name;

            return new TechnicianResponseDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                TechnicianCategory = user.TechnicianCategory == null
                    ? null
                    : new TCategoryUserResponseDTO
                    {
                        Id = user.TechnicianCategory.Id,
                        Name = catName
                    }
            };
        }

        public static AssignTechnicianResponseDTO ToAssignTechnicianResponse(
            MaintenanceRequest request, string language = "ar")
        {
            if (request == null) return null;

            return new AssignTechnicianResponseDTO
            {
                MaintenanceRequestId = request.Id,
                Technician = request.AssignedTechnician == null
                    ? null
                    : ToTechnicianResponse(request.AssignedTechnician, language),
                AssignedAtUtc = request.AssignedAtUtc ?? DateTime.UtcNow
            };
        }

        // 1) النسخة الافتراضية: تبني URL نسبي للصورة الأولى (مع تفضيل IsPrimary)
        public static TechnicianAssignedRequestResponseDTO ToTechnicianAssigned(
            MaintenanceRequest r,
            string language = "ar")
            => ToTechnicianAssigned(r, language, fileName => $"/Images/{fileName}");

        // 2) نسخة تقبل مولّد روابط مخصّص (اختياري)
        public static TechnicianAssignedRequestResponseDTO ToTechnicianAssigned(
            MaintenanceRequest r,
            string language,
            Func<string, string> urlBuilder)
        {
            if (r == null) return null;

            // اسم نوع المشكلة من الترجمات
            var ptName = r.ProblemType?.Translations?
                            .FirstOrDefault(t => t.Language == language)?.Name
                         ?? r.ProblemType?.Translations?.FirstOrDefault()?.Name;

            // اختر أول ملف صورة مع تفضيل IsPrimary
            string? firstImageFile = r.Images != null && r.Images.Count > 0
                ? r.Images
                    .OrderByDescending(i => i.IsPrimary)
                    .Select(i => i.FileName)
                    .FirstOrDefault()
                : null;

            return new TechnicianAssignedRequestResponseDTO
            {
                Id = r.Id,
                Title = r.Title,
                Address = r.Address,
                CaseType = MaintenanceRequestMapper.GetCaseTypeName(r.CaseType, language),
                Priority = r.Priority.ToString(),
                ProblemTypeId = r.ProblemTypeId,
                ProblemTypeName = ptName,
                // كان سابقًا: $"https://localhost:7127/Images/{r.MainImage}"
                MainImage = firstImageFile == null ? null : urlBuilder(firstImageFile),
                CreatedAt = r.CreatedAt,
                AssignedAtUtc = r.AssignedAtUtc
            };
        }
    }
}
