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
            string createdByUserId,
            string? mainImageSavedPath = null)
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
                CreatedAt = DateTime.UtcNow,
                MainImage = mainImageSavedPath
            };
        }

        public static MaintenanceRequestResponseDTO ToResponse(MaintenanceRequest e)
        {
            return new MaintenanceRequestResponseDTO
            {
                Id = e.Id,
                RequestNumber = e.RequestNumber,
                Title = e.Title,
                Description = e.Description,
                Priority = e.Priority,
                CaseType = e.CaseType,
                Address = e.Address,
                CreatedByUserId = e.CreatedByUserId,
                CreatedAt = e.CreatedAt,
                MainImage = e.MainImage
            };
        }
    }
}
