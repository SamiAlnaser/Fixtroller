using Fixtroller.BLL.Mapping;
using Fixtroller.BLL.Services.FileService;
using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.MaintenanceRequestepository;
using Fixtroller.DAL.Repositories.UserRepository.TechnicianRepositorirs;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.MaintenanceRequestServices
{
    public class MaintenanceRequestService : GenericService<MaintenanceRequestRequestDTO, MaintenanceRequestResponseDTO, MaintenanceRequest>, IMaintenanceRequestService
    {
        private readonly IMaintenanceRequestRepository _repository;
        private readonly ITechnicianRepository _techRepo;
        private readonly IFileService _fileService;
        public MaintenanceRequestService(IMaintenanceRequestRepository repository, ITechnicianRepository techRepo, IFileService fileService) : base(repository)
        {
            _repository = repository;
            _techRepo = techRepo;
            _fileService = fileService;
        }
        public async Task<int> CreateWithFile(MaintenanceRequestRequestDTO request, string userId)
        {
            string? savedPath = null;
            if (request.MainImage is not null && request.MainImage.Length > 0)
            {
                savedPath = await _fileService.UploadAsync(request.MainImage);
            }

            var entity = MaintenanceRequestMapper.ToEntity(request, userId, savedPath);

            entity.RequestNumber = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % int.MaxValue);

            return await _repository.AddAsync(entity);
        }

        public async Task<IEnumerable<MaintenanceRequestResponseDTO>> GetMineAsync(string userId)
        {
            var data = await _repository.Query(
                            asTracking: false,
                            include: null,
                            predicate: x => x.CreatedByUserId == userId)
                        .OrderByDescending(x => x.CreatedAt)
                        .ToListAsync();

            return data.Select(MaintenanceRequestMapper.ToResponse);
        }

        public async Task<IEnumerable<MaintenanceRequestResponseDTO>> GetAllAsync()
        {
            var data = await _repository.Query(asTracking: false)
                                  .OrderByDescending(x => x.CreatedAt)
                                  .ToListAsync();

            return data.Select(MaintenanceRequestMapper.ToResponse);
        }

        public async Task<MaintenanceRequestResponseDTO?> GetByIdAsync(int id)
        {
            var e = await _repository.Query(
                            asTracking: false,
                            predicate: x => x.Id == id)
                        .FirstOrDefaultAsync();

            return e is null ? null : MaintenanceRequestMapper.ToResponse(e);
        }

        public async Task<(AssignTechnicianResponseDTO? Response, string MessageKey)> AssignTechnicianAsync(
            int requestId, string technicianUserId, string language = "ar")
        {
            var request = await _repository.GetForAssignmentAsync(requestId);
            if (request is null) return (null, "Request_NotFound");


            var tech = await _techRepo.GetByIdAsync(technicianUserId); // AsNoTracking OK
            if (tech is null) return (null, "Technician_NotFound");

            var isTechnician = await _techRepo.IsInRoleAsync(technicianUserId, "Technician");
            if (!isTechnician) return (null, "User_NotTechnician");

            request.AssignedTechnicianUserId = technicianUserId;
            request.AssignedAtUtc = DateTime.UtcNow;
            if (request.CaseType == CaseType.Submitted)
                request.CaseType = CaseType.Processing;

            await _repository.CommitAsync();

            var loaded = await _repository.GetForAssignmentAsync(requestId);
            return (TechnicianMappings.ToAssignTechnicianResponse(loaded!, language), "Technician_Assigned");
        }
        public async Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> ChangeCaseAsync(
           int requestId,
           CaseType newCase,
           string userId,
           string userRole,
           string language = "ar")
        {
            var r = await _repository.GetForUpdateAsync(requestId);
            if (r is null) return (null, "Request_NotFound");

            if (r.CaseType == newCase)
                return (MaintenanceRequestMapper.ToResponse(r), "Case_NoChange"); // لا تغيير

            // حالات تُدار تلقائياً
            if (newCase is CaseType.Submitted or CaseType.Processing)
                return (null, "Case_AutoManaged");

            bool isManager = userRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            bool isAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            bool isTechnician = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            bool isOwner = r.CreatedByUserId == userId;

            // المالك (بغض النظر عن دوره): Modified, Cancelled, Reopened, Completed
            if (isOwner && !(isManager || isAdmin))
            {
                var allowedOwner = new[] { CaseType.Modified, CaseType.Cancelled, CaseType.Reopened, CaseType.Completed };
                if (!allowedOwner.Contains(newCase))
                    return (null, "Case_NotAllowedForOwner");

                r.CaseType = newCase;
                //r.UpdatedAt = DateTime.UtcNow;  ////////////////////////// لازم تنعمل من entity (مع باقي تفاصيل العرض و الاضافة و الملحفات)
                await _repository.CommitAsync();
                return (MaintenanceRequestMapper.ToResponse(r), "Case_Changed");
            }


            if (isTechnician)
            {
                if (r.AssignedTechnicianUserId != userId)
                    return (null, "Request_NotAssignedToYou");

                var allowedTech = new[] { CaseType.ResourcesNeeded, CaseType.Processed };
                if (!allowedTech.Contains(newCase))
                    return (null, "Case_NotAllowedForTechnician");

                r.CaseType = newCase;
                //r.UpdatedAt = DateTime.UtcNow;////////////////////////////
                await _repository.CommitAsync();
                return (MaintenanceRequestMapper.ToResponse(r), "Case_Changed");
            }

            if (isManager || isAdmin)
            {
                var allowedMgr = new[] { CaseType.ResourcesNeeded, CaseType.Processed, CaseType.Completed };
                if (!allowedMgr.Contains(newCase))
                    return (null, "Case_NotAllowedForManager");

                r.CaseType = newCase;
                //r.UpdatedAt = DateTime.UtcNow;////////////////////////////////////////
                await _repository.CommitAsync();
                return (MaintenanceRequestMapper.ToResponse(r), "Case_Changed");
            }

            return (null, "Forbidden");
        }
    }
}
    


    

