using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.TechnicianServices
{
    public interface ITechnicianService
    {
        Task<IReadOnlyList<TechnicianResponseDTO>> GetAsync(
            TechniciansFilterRequestDTO filter,
            CancellationToken ct = default);

        Task<bool> UpdateTechnicianCategoryAsync(
            UpdateTechnicianCategoryRequestDTO dto,
            CancellationToken ct = default);

        Task<IReadOnlyList<TechnicianAssignedRequestResponseDTO>> GetMyAssignedAsync(
            string technicianUserId,
            string language = "ar",
            CancellationToken ct = default);

        Task<IReadOnlyList<TechnicianResponseDTO>> GetByCategoryAsync(
            int categoryId,
            string? search,
            string language,
            CancellationToken ct = default);
    }
}

