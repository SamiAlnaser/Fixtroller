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
        // =========================
        // 1) ردّ تعيين الفنيين
        // =========================
        public static AssignTechnicianResponseDTO ToAssignTechnicianResponse(
            MaintenanceRequest request, string language = "ar")
        {
            if (request == null) return null;

            // ابني قائمة الفنيين النشطين من جدول الربط
            var list = (request.Technicians ?? Enumerable.Empty<MaintenanceRequestTechnician>())
                .Where(t => t.UnassignedAtUtc == null)
                .OrderByDescending(t => t.AssignedAtUtc)
                .Select(t => new TechnicianResponseDTO
                {
                    Id = t.TechnicianUserId,
                    AssignedAtUtc = t.AssignedAtUtc,
                    ExpectedDuration = t.ExpectedDuration
                })
                .ToList();

            var res = new AssignTechnicianResponseDTO
            {
                MaintenanceRequestId = request.Id,
                ActiveTechnicians = list
            };

            // توافق قديم: لو فني واحد رجّع أيضًا الحقول القديمة
            if (list.Count == 1)
            {
                res.Technician = list[0];
                res.AssignedAtUtc = list[0].AssignedAtUtc;
            }

            return res;
        }

        // =========================================
        // 2) بطاقة طلب مع فني (لأول فني نشِط إن وُجد)
        // =========================================
        public static TechnicianAssignedRequestResponseDTO ToTechnicianAssigned(
            MaintenanceRequest r,
            string language = "ar")
            => ToTechnicianAssigned(r, language, fileName => $"/Images/{fileName}");

        public static TechnicianAssignedRequestResponseDTO ToTechnicianAssigned(
            MaintenanceRequest r,
            string language,
            Func<string, string> urlBuilder)
        {
            if (r == null) return null;

            var ptName = r.ProblemType?.Translations?
                            .FirstOrDefault(t => t.Language == language)?.Name
                         ?? r.ProblemType?.Translations?.FirstOrDefault()?.Name;

            string? firstImageFile = (r.Images != null && r.Images.Count > 0)
                ? r.Images.OrderByDescending(i => i.IsPrimary).Select(i => i.FileName).FirstOrDefault()
                : null;

            var firstLink = r.Technicians?
                .Where(t => t.UnassignedAtUtc == null)
                .OrderByDescending(t => t.AssignedAtUtc)
                .FirstOrDefault();

            return new TechnicianAssignedRequestResponseDTO
            {
                Id = r.Id,
                Title = r.Title,
                Address = r.Address,
                CaseType = MaintenanceRequestMapper.GetCaseTypeName(r.CaseType, language),
                Priority = r.Priority.ToString(),
                ProblemTypeId = r.ProblemTypeId,
                ProblemTypeName = ptName,
                MainImage = firstImageFile == null ? null : urlBuilder(firstImageFile),
                CreatedAt = r.CreatedAt,
                AssignedAtUtc = firstLink?.AssignedAtUtc
            };
        }

        // =========================
        // 3) فني مفرد (عام)
        // =========================
        public static TechnicianResponseDTO ToTechnicianResponse(ApplicationUser user, string language = "ar")
        {
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
    }
}
    
