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

        Task<IEnumerable<TechnicianListItemDTO>> GetWithMetricsAsync(
    string language,
    int? technicianCategoryId,
    string? search,
    CancellationToken ct = default);

        Task<bool> UpdateTechnicianCategoryAsync(
            UpdateTechnicianCategoryRequestDTO dto,
            CancellationToken ct = default);

        Task<TechnicianBoardDTO> GetMyAssignedAsync(
            string technicianUserId, string language, CancellationToken ct = default);


        Task<IReadOnlyList<TechnicianResponseDTO>> GetByCategoryAsync(
            int categoryId,
            string? search,
            string language,
            CancellationToken ct = default);
    }
}

