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
    public interface IMaintenanceRequestService
         : IGenericService<MaintenanceRequestRequestDTO, MaintenanceRequestResponseDTO, MaintenanceRequest>
    {
        Task<int> CreateWithFile(
            MaintenanceRequestRequestDTO request,
            string userId,
            CancellationToken ct = default);

        Task<IEnumerable<MaintenanceRequestResponseDTO>> GetMineAsync(
            string userId,
            string role,
            string language,
            CancellationToken ct = default);

        // للإدارة (ولو بدك تحدد المالك لإظهار isOwner صح مرّر currentUserId)
        Task<IEnumerable<MaintenanceRequestResponseDTO>> GetAllAsync(
            string role,
            string language,
            string? currentUserId = null,
            CancellationToken ct = default);

        Task<MaintenanceRequestResponseDTO?> GetByIdAsync(
            int id,
            string userId,
            string role,
            string language,
            CancellationToken ct = default);

        Task<(AssignTechnicianResponseDTO? Response, string MessageKey)> AssignTechnicianAsync(
            int requestId,
            string technicianUserId,
            string language = "ar",
            CancellationToken ct = default);

        Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> ChangeCaseAsync(
            int requestId,
            ChangeCaseTypeRequestDTO dto,
            string userId,
            string userRole,
            bool preferOwnerPath = false,
            string language = "ar",
            CancellationToken ct = default);

        Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> UpdateMineAsync(
            int id,
            string userId,
            string role,
            MaintenanceRequestUpdateDTO dto,
            string language = "ar",
            CancellationToken ct = default);

        Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> AddNoteAsync(
            int requestId,
            string userId,
            string userRole,
            AddNoteRequestDTO dto,
            string language = "ar",
            CancellationToken ct = default);
    }
}

