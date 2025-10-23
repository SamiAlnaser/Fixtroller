using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.MaintenanceRequestServices
{
    public interface IMaintenanceRequestService : IGenericService<MaintenanceRequestRequestDTO, MaintenanceRequestResponseDTO , MaintenanceRequest>
    {
        Task<int> CreateWithFile(MaintenanceRequestRequestDTO request , string userId);
        Task<IEnumerable<MaintenanceRequestResponseDTO>> GetMineAsync(string userId, string role);
        Task<IEnumerable<MaintenanceRequestResponseDTO>> GetAllAsync(string role); // لإدارة
        Task<MaintenanceRequestResponseDTO?> GetByIdAsync(int id, string userId, string role, string language);
       
        Task<(AssignTechnicianResponseDTO? Response, string MessageKey)> AssignTechnicianAsync(int requestId, string technicianUserId, string language = "ar");
        Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> ChangeCaseAsync(int id, CaseType newCase, string userId, string userRole, string language = "ar");

        Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)>
        UpdateMineAsync(int id, string userId, string role, MaintenanceRequestUpdateDTO dto, string language = "ar");

    }
}

