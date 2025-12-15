using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.PagedResultDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.UsersDTOs.Responses;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.TechnicianServices
{
    public interface ITechnicianService
    {

        Task<PagedResultDTO<TechnicianBoardDTO>> GetMyAssignedAsync(
            string technicianUserId,
            string language,
            int pageNumber = 1,
            int pageSize = 10,
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            int? requestId = null,
            CancellationToken ct = default);

        Task<PagedResultDTO<TechnicianResponseDTO>> GetByCategoryAsync(
         int categoryId,
         string? search,
         string language,
         int pageNumber = 1,
         int pageSize = 10,
         CancellationToken ct = default);


        Task<PagedResultDTO<TechnicianListItemDTO>> GetWithMetricsAsync(
            string language,
            int? technicianCategoryId,
            string? search,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default);


        Task<bool> UpdateTechnicianCategoryAsync(
            UpdateTechnicianCategoryRequestDTO dto,
            CancellationToken ct = default);
        Task<bool> ClearTechnicianCategoryAsync(
    ClearTechnicianCategoryRequestDTO dto,
    CancellationToken ct = default);



    }
}

