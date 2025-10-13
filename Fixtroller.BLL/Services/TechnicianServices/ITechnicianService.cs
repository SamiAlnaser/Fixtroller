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
        Task<IReadOnlyList<TechnicianResponseDTO>> GetAsync(TechniciansFilterRequestDTO filter);
        Task<bool> UpdateTechnicianCategoryAsync(UpdateTechnicianCategoryRequestDTO dto);
        Task<IReadOnlyList<TechnicianAssignedRequestResponseDTO>> GetMyAssignedAsync(string technicianUserId, string language = "ar");
        Task<IReadOnlyList<TechnicianResponseDTO>> GetByCategoryAsync(int categoryId, string? search, string language);
    }
}

