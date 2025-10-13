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
        Task<IEnumerable<MaintenanceRequestResponseDTO>> GetMineAsync(string userId);
        Task<IEnumerable<MaintenanceRequestResponseDTO>> GetAllAsync(); // لإدارة
        Task<MaintenanceRequestResponseDTO?> GetByIdAsync(int id);
       
        Task<(AssignTechnicianResponseDTO? Response, string MessageKey)> AssignTechnicianAsync(int requestId, string technicianUserId, string language = "ar");

      
        Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> ChangeCaseAsync(int id, CaseType newCase, string userId, string userRole, string language = "ar");
    }
}

