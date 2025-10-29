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
            var entity = MaintenanceRequestMapper.ToEntity(request, userId);

            var fileNames = new List<string>();
            if (request.Images != null)
            {
                foreach (var f in request.Images)
                {
                    if (f != null && f.Length > 0)
                        fileNames.Add(await _fileService.UploadAsync(f));
                }
            }

            for (int i = 0; i < fileNames.Count; i++)
                entity.Images.Add(new MaintenanceRequestImage { FileName = fileNames[i], IsPrimary = (i == 0) });

            if (entity.Images.Count > 0 && !entity.Images.Any(i => i.IsPrimary))
                entity.Images.First().IsPrimary = true;

            var id = await _repository.AddAsync(entity);
            return id;
        }
        public async Task<IEnumerable<MaintenanceRequestResponseDTO>> GetMineAsync(string userId, string role)
        {
            var data = await _repository.Query(
                                asTracking: false,
                                predicate: x => x.CreatedByUserId == userId)
                            .OrderByDescending(x => x.CreatedAt)
                            .ToListAsync();

            return data.Select(x => MaintenanceRequestMapper.ToResponse(x, role));
        }

        public async Task<IEnumerable<MaintenanceRequestResponseDTO>> GetAllAsync(string role)
        {
            var data = await _repository.Query(asTracking: false)
                                  .OrderByDescending(x => x.CreatedAt)
                                  .ToListAsync();

            return data.Select(x => MaintenanceRequestMapper.ToResponse(x, role));
        }

        public async Task<MaintenanceRequestResponseDTO?> GetByIdAsync(int id, string userId, string role, string language = "ar")
        {
            var e = await _repository.Query(
                            asTracking: false,
                            predicate: x =>
                                x.Id == id &&
                                (
                                    role == "Admin" ||
                                    role == "MaintenanceManager" ||
                                    (role == "Employee" && (x.CreatedByUserId == userId)) ||
                                    (role == "Technician" && (x.AssignedTechnicianUserId == userId || x.CreatedByUserId == userId))
                                ),
                                include: q => q.Include(r => r.Images)
                                .Include(r => r.Notes)
                                .Include(r => r.AssignedTechnician)
                            .ThenInclude(t => t.TechnicianCategory)
                                .ThenInclude(c => c.Translations)
                                ).FirstOrDefaultAsync();


            if (e is null) return null;


            var isOwner = string.Equals(e.CreatedByUserId, userId, StringComparison.Ordinal);
            return MaintenanceRequestMapper.ToResponse(e, role, _fileService.GetPublicUrl, language, isOwner);

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
            ChangeCaseTypeRequestDTO dto,
            string userId,
            string userRole,
            bool preferOwnerPath,
            string language = "ar")
        {
            var r = await _repository.GetForUpdateAsync(requestId);
            if (r is null) return (null, "Request_NotFound");

            var newCase = dto.NewCaseType;

            bool isManager = userRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            bool isAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            bool isTechnician = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            bool isOwner = r.CreatedByUserId == userId;

            var useOwnerPath = isOwner && (preferOwnerPath || !(isManager || isAdmin || isTechnician));

            var author = InferAuthor(isOwner, isTechnician, isManager, isAdmin);

            if (r.CaseType == newCase)
            {
                var respNoChange = MaintenanceRequestMapper.ToResponse(
                    r, userRole, _fileService.GetPublicUrl, language, isOwner);
                return (respNoChange, "Case_Changed"); // لا تغيير
            }

            // حالات تُدار تلقائيًا
            if (newCase is CaseType.Submitted or CaseType.Processing or CaseType.Modified)
                return (null, "Case_AutoManaged");

            // تحضير إلزام الملاحظة
            bool needNote = newCase is CaseType.Reopened or CaseType.ResourcesNeeded;
            if (needNote && string.IsNullOrWhiteSpace(dto.NoteText))
                return (null, "Note_Required_For_This_Case");

            NoteType inferredType = newCase switch
            {
                CaseType.Reopened => NoteType.ReopenReason,
                CaseType.ResourcesNeeded => NoteType.HelpRequest,
                _ => dto.NoteType ?? NoteType.General
            };

            // المالك
            if (useOwnerPath)
                {
                var allowedOwner = new[] { CaseType.Cancelled, CaseType.Reopened, CaseType.Completed };
                if (!allowedOwner.Contains(newCase))
                    return (null, "Case_NotAllowedForOwner");

                r.CaseType = newCase;

                if (needNote || !string.IsNullOrWhiteSpace(dto.NoteText))
                {
                    r.Notes.Add(MaintenanceRequestMapper.ToNote(
                        dto.NoteText!, inferredType, author, userId, r.Id));
                }

                r.UpdatedAt = DateTime.UtcNow;
                await _repository.CommitAsync();

                var resp = MaintenanceRequestMapper.ToResponse(
                    r, userRole, _fileService.GetPublicUrl, language, isOwner);
                return (resp, "Case_Changed");
            }

            // الفني
            if (isTechnician)
            {
                if (r.AssignedTechnicianUserId != userId)
                    return (null, "Request_NotAssignedToYou");

                var allowedTech = new[] { CaseType.ResourcesNeeded, CaseType.ManagerReview };
                if (!allowedTech.Contains(newCase))
                    return (null, "Case_NotAllowedForTechnician");

                r.CaseType = newCase;

                if (needNote || !string.IsNullOrWhiteSpace(dto.NoteText))
                {
                    r.Notes.Add(MaintenanceRequestMapper.ToNote(
                        dto.NoteText!, inferredType, author, userId, r.Id));
                }

                r.UpdatedAt = DateTime.UtcNow;
                await _repository.CommitAsync();

                var resp = MaintenanceRequestMapper.ToResponse(
                    r, userRole, _fileService.GetPublicUrl, language, isOwner);
                return (resp, "Case_Changed");
            }

            // المدير أو الأدمن
            if (isManager || isAdmin)
            {
                var allowedMgr = new[] { CaseType.ResourcesNeeded, CaseType.Processed, CaseType.Completed };
                if (!allowedMgr.Contains(newCase))
                    return (null, "Case_NotAllowedForManager");

                r.CaseType = newCase;

                if (needNote || !string.IsNullOrWhiteSpace(dto.NoteText))
                {
                    r.Notes.Add(MaintenanceRequestMapper.ToNote(
                        dto.NoteText!, inferredType, author, userId, r.Id));
                }

                r.UpdatedAt = DateTime.UtcNow;
                await _repository.CommitAsync();

                var resp = MaintenanceRequestMapper.ToResponse(
                    r, userRole, _fileService.GetPublicUrl, language, isOwner);
                return (resp, "Case_Changed");
            }

            return (null, "Forbidden");
        }


        public async Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)>
    AddNoteAsync(int requestId, string userId, string userRole, AddNoteRequestDTO dto, string language = "ar")
        {
            var r = await _repository.GetForUpdateAsync(requestId);
            if (r is null) return (null, "Request_NotFound");

            var isOwner = string.Equals(r.CreatedByUserId, userId, StringComparison.Ordinal);
            var isTech = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            var isMgr = userRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            var author = InferAuthor(isOwner, isTech, isMgr, isAdmin);

            var lockedForManager = r.CaseType == CaseType.Completed || r.CaseType == CaseType.Cancelled;

            if (isMgr && lockedForManager && !isAdmin)
                return (null, "Notes_Disabled_For_Manager_In_FinalState");

            if (isTech && r.AssignedTechnicianUserId != userId)
                return (null, "Request_NotAssignedToYou");

            if (!isOwner && !isTech && !isMgr && !isAdmin)
                return (null, "Forbidden");

            if (string.IsNullOrWhiteSpace(dto.Text))
                return (null, "Note_Text_Required");


            var noteType = dto.Type ?? NoteType.General;
            

            var note = MaintenanceRequestMapper.ToNote(
               dto.Text!,
               noteType,
                author,
               userId,
               r.Id
           );

            r.Notes.Add(note);
            r.UpdatedAt = DateTime.UtcNow;
            await _repository.CommitAsync();

            // رجّع الطلب مع الملاحظات
            var role = userRole;
            return (MaintenanceRequestMapper.ToResponse(r, role, _fileService.GetPublicUrl, language,isOwner), "Note_Added");
        }


        public async Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)>
            UpdateMineAsync(int id, string userId, string role, MaintenanceRequestUpdateDTO dto, string language = "ar")
        {
            var r = await _repository.GetForUpdateAsync(id);
            if (r is null) return (null, "Request_NotFound");

            // المالك فقط (بغض النظر عن دوره)
            if (!string.Equals(r.CreatedByUserId, userId, StringComparison.Ordinal))
                throw new UnauthorizedAccessException("Forbidden");

            // الحالات المسموح فيها التعديل
            var editable = new HashSet<CaseType>
    {
        CaseType.Submitted,
        CaseType.ManagerReview,
        CaseType.Reopened,
        CaseType.Modified
    };
            if (!editable.Contains(r.CaseType))
                return (null, "Request_NotEditableInThisState");

            // تعديل الحقول النصية
            if (!string.IsNullOrWhiteSpace(dto.Title)) r.Title = dto.Title.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Description)) r.Description = dto.Description.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Address)) r.Address = dto.Address.Trim();
            if (dto.Priority.HasValue) r.Priority = dto.Priority.Value;
            if (dto.ProblemTypeId.HasValue) r.ProblemTypeId = dto.ProblemTypeId.Value;

            // حذف صور
            if (dto.RemoveImageIds != null && dto.RemoveImageIds.Count > 0 && r.Images.Count > 0)
            {
                var toRemove = r.Images.Where(i => dto.RemoveImageIds.Contains(i.Id)).ToList();
                foreach (var img in toRemove)
                {
                    await _fileService.DeleteAsync(img.FileName);
                    r.Images.Remove(img);
                }
            }

            // إضافة صور
            if (dto.NewImages != null && dto.NewImages.Count > 0)
            {
                foreach (var f in dto.NewImages)
                {
                    if (f != null && f.Length > 0)
                    {
                        var name = await _fileService.UploadAsync(f);
                        r.Images.Add(new MaintenanceRequestImage { FileName = name, IsPrimary = false });
                    }
                }
            }

            // ضمان وجود صورة أساسية واحدة
            if (r.Images.Count > 0 && !r.Images.Any(i => i.IsPrimary))
                r.Images.First().IsPrimary = true;

            // انتقال الحالة وتحديث الوقت
            r.CaseType = CaseType.Modified;
            r.UpdatedAt = DateTime.UtcNow; // أو UpdatedAtUtc حسب كيانك

            await _repository.CommitAsync();
            return (MaintenanceRequestMapper.ToResponse(r, role, _fileService.GetPublicUrl), "Request_Updated");

        }

        private static NoteAuthor InferAuthor(bool isOwner, bool isTech, bool isMgr, bool isAdmin)
        {
            if (isAdmin) return NoteAuthor.Admin;
            if (isMgr) return NoteAuthor.Manager;
            if (isTech) return NoteAuthor.Technician;
            return NoteAuthor.Owner;
        }


    }
}
    


    

