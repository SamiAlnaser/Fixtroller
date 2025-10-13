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
        public static TechnicianAssignedRequestResponseDTO ToTechnicianAssigned(
    MaintenanceRequest r,
    string language = "ar")
        {
            if (r == null) return null;

            // اسم نوع المشكلة من الترجمات
            var ptName = r.ProblemType?.Translations?
                            .FirstOrDefault(t => t.Language == language)?.Name
                         ?? r.ProblemType?.Translations?.FirstOrDefault()?.Name;

            return new TechnicianAssignedRequestResponseDTO
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                Title = r.Title,
                Address = r.Address,
                CaseType = r.CaseType.ToString(),
                Priority = r.Priority.ToString(),
                ProblemTypeId = r.ProblemTypeId,
                ProblemTypeName = ptName,
                MainImage = $"https://localhost:7127/Images/{r.MainImage}",
                CreatedAt = r.CreatedAt,
                AssignedAtUtc = r.AssignedAtUtc
            };
        }
    }
}
