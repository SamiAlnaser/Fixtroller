using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.PagedResultDTOs.Responses;
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


        Task<PagedResultDTO<MaintenanceRequestListMineDTO>> GetMineAsync(string userId,string role,string language,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken ct = default);

        Task<PagedResultDTO<MaintenanceRequestListAllDTO>> GetAllAsync(string role,string language,int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default);

        Task<MaintenanceRequestResponseDTO?> GetByIdAsync(
            int id,
            string userId,
            string role,
            string language,
            CancellationToken ct = default);

        // تعيين فني مفرد (توافق خلفي) — يلتف على AssignTechniciansAsync
        Task<(AssignTechnicianResponseDTO? Response, string MessageKey)> AssignTechnicianAsync(
            int requestId,
            string technicianUserId,
            int? expectedDuration,
            string language = "ar",
            CancellationToken ct = default);

        // NEW: تعيين قائمة فنيين دفعة واحدة (يُضيف/يزيل مقارنةً بالحالي)
        Task<(AssignTechnicianResponseDTO? Response, string MessageKey)> AssignTechniciansAsync(
            int requestId,
            IEnumerable<string> technicianUserIds,
            int? expectedDuration,
            string language = "ar",
            CancellationToken ct = default);

        // NEW: إزالة فني واحد من الطلب (+ سيُوقف مؤقّته النشط لهذا الطلب)
        Task<(bool ok, string messageKey)> RemoveTechnicianAsync(
            int requestId,
            string technicianUserId,
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

        // CHANGED: صار يطلب technicianUserId صراحةً
        Task<(bool ok, string messageKey)> StartWorkAsync(
            int requestId,
            string technicianUserId,
            string callerUserId,
            string callerRole,
            CancellationToken ct = default);

        Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> AddImagesAsync(
    int requestId,
    string userId,
    string userRole,
    AddImagesRequestDTO dto,
    string language = "ar",
    CancellationToken ct = default);




    }

}


