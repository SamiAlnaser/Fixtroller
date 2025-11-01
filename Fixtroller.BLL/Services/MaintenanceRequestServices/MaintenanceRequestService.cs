using Fixtroller.BLL.Mapping;
using Fixtroller.BLL.Services.FileService;
using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.MaintenanceRequestepository;
using Fixtroller.DAL.Repositories.UserRepository.TechnicianRepositorirs;
using Fixtroller.DAL.UnitOfWork;
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
        private readonly IUnitOfWork _uow;
        private readonly IWorkTimeRepository _workRepo;
        private readonly IMaintenanceRequestTechnicianRepository _reqTechRepo; 

        public MaintenanceRequestService(
            IMaintenanceRequestRepository repository,
            ITechnicianRepository techRepo,
            IFileService fileService,
            IUnitOfWork uow,
            IWorkTimeRepository workRepo,
            IMaintenanceRequestTechnicianRepository reqTechRepo 
        ) : base(repository, uow)
        {
            _repository = repository;
            _techRepo = techRepo;
            _fileService = fileService;
            _uow = uow;
            _workRepo = workRepo;
            _reqTechRepo = reqTechRepo; // NEW
        }

        public async Task<int> CreateWithFile(MaintenanceRequestRequestDTO request, string userId, CancellationToken ct = default)
        {
            // جهّز الكيان
            var entity = MaintenanceRequestMapper.ToEntity(request, userId);

            // 1) ارفع الملفات "خارج" الترانزاكشن + لائحة للتعويض عند الفشل
            var uploaded = new List<string>();
            if (request.Images != null)
            {
                foreach (var f in request.Images)
                {
                    if (f != null && f.Length > 0)
                        uploaded.Add(await _fileService.UploadAsync(f, ct));
                }
            }

            // اربط الصور بالكيان (الأولى Primary)
            for (int i = 0; i < uploaded.Count; i++)
                entity.Images.Add(new MaintenanceRequestImage { FileName = uploaded[i], IsPrimary = (i == 0) });

            if (entity.Images.Count > 0 && !entity.Images.Any(i => i.IsPrimary))
                entity.Images.First().IsPrimary = true;

            await _uow.BeginTransactionAsync(ct);
            try
            {
                await _repository.AddAsync(entity);
                await _uow.SaveAndCommitAsync(ct);
                return entity.Id;
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                foreach (var name in uploaded)
                {
                    try { await _fileService.DeleteAsync(name, ct); } catch { }
                }
                throw;
            }
        }

        public async Task<IEnumerable<MaintenanceRequestResponseDTO>> GetMineAsync(
            string userId,
            string role,
            string language,
            CancellationToken ct = default)
        {
            var data = await _repository.Query(
                                asTracking: false,
                                predicate: x => x.CreatedByUserId == userId)
                            .OrderByDescending(x => x.CreatedAt)
                            .ToListAsync(ct);

            return data.Select(x =>
                MaintenanceRequestMapper.ToResponse(
                    x,
                    role,
                    _fileService.GetPublicUrl,
                    language,
                    isOwner: true));
        }

        public async Task<IEnumerable<MaintenanceRequestResponseDTO>> GetAllAsync(
            string role,
            string language,
            string? currentUserId = null,
            CancellationToken ct = default)
        {
            var data = await _repository.Query(asTracking: false)
                                        .OrderByDescending(x => x.CreatedAt)
                                        .ToListAsync(ct);

            return data.Select(x =>
                MaintenanceRequestMapper.ToResponse(
                    x,
                    role,
                    _fileService.GetPublicUrl,
                    language,
                    isOwner: currentUserId != null && x.CreatedByUserId == currentUserId));
        }

        public async Task<MaintenanceRequestResponseDTO?> GetByIdAsync(
            int id,
            string userId,
            string role,
            string language = "ar",
            CancellationToken ct = default)
        {
            // ثوابت تُترجم لـ SQL
            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            var isManager = string.Equals(role, "MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isEmployee = string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase);
            var isTechnician = string.Equals(role, "Technician", StringComparison.OrdinalIgnoreCase);

            var e = await _repository.Query(
                        asTracking: false,
                        predicate: x =>
                            x.Id == id &&
                            (
                                isAdmin ||
                                isManager ||
                                (isEmployee && x.CreatedByUserId == userId) ||
                                (isTechnician && (x.CreatedByUserId == userId ||
                                                  x.Technicians.Any(t => t.UnassignedAtUtc == null && t.TechnicianUserId == userId)))
                            ),
                        include: q => q
                            .Include(r => r.Images)
                            .Include(r => r.Notes)
                            .Include(r => r.Technicians.Where(t => t.UnassignedAtUtc == null))
                    )
                    .FirstOrDefaultAsync(ct);

            if (e is null) return null;

            var isOwner = string.Equals(e.CreatedByUserId, userId, StringComparison.Ordinal);
            return MaintenanceRequestMapper.ToResponse(e, role, _fileService.GetPublicUrl, language, isOwner);
        }

        public async Task<(AssignTechnicianResponseDTO? Response, string MessageKey)> AssignTechniciansAsync(
            int requestId, IEnumerable<string> technicianUserIds, string language = "ar", CancellationToken ct = default)
        {
            var request = await _repository.GetForAssignmentAsync(requestId, ct);
            if (request is null) return (null, "Request_NotFound");

            var list = (technicianUserIds ?? Enumerable.Empty<string>())
                       .Where(s => !string.IsNullOrWhiteSpace(s))
                       .Select(s => s.Trim())
                       .Distinct(StringComparer.Ordinal)
                       .ToList();

            if (list.Count == 0)
                return (null, "Technician_ListEmpty");

            // تحقق من صحة كل معرّف ودوره
            foreach (var tid in list)
            {
                var tech = await _techRepo.GetByIdAsync(tid, ct);
                if (tech is null) return (null, "Technician_NotFound");

                var isTechnician = await _techRepo.IsInRoleAsync(tid, "Technician", ct);
                if (!isTechnician) return (null, "User_NotTechnician");
            }

            await _uow.BeginTransactionAsync(ct);
            try
            {
                // التعرّف على الفنيين الذين سيتم شطبهم لإيقاف مؤقتاتهم بعد التحديث
                var current = await _reqTechRepo.GetActiveTechniciansAsync(requestId, ct);
                var removed = current.Except(list, StringComparer.Ordinal).ToList();

                // مزامنة التعيينات: إضافة الجديد وشطب غير الموجود
                await _reqTechRepo.SetActiveListAsync(requestId, list, ct);

                // أوقف مؤقّتات الفنيين الذين أُزيلوا من التعيين
                foreach (var tid in removed)
                    await _workRepo.StopActiveForRequestAndTechAsync(requestId, tid, ct);

                // لو Submitted ارفعها إلى Processing (نفس سلوكك)
                if (request.CaseType == CaseType.Submitted)
                    request.CaseType = CaseType.Processing;

                request.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveAndCommitAsync(ct);

                var loaded = await _repository.GetForAssignmentAsync(requestId, ct);
                return (TechnicianMappings.ToAssignTechnicianResponse(loaded!, language), "Technicians_Assigned");
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<(AssignTechnicianResponseDTO? Response, string MessageKey)> AssignTechnicianAsync(
            int requestId, string technicianUserId, string language = "ar", CancellationToken ct = default)
        {
            var request = await _repository.GetForAssignmentAsync(requestId, ct);
            if (request is null) return (null, "Request_NotFound");

            var tech = await _techRepo.GetByIdAsync(technicianUserId, ct);
            if (tech is null) return (null, "Technician_NotFound");

            var isTechnician = await _techRepo.IsInRoleAsync(technicianUserId, "Technician", ct);
            if (!isTechnician) return (null, "User_NotTechnician");

            // إن كان مُعيَّن نشطًا أصلًا، لا تغيّر شيء
            var already = await _reqTechRepo.IsActiveAssignedAsync(requestId, technicianUserId, ct);
            if (already)
            {
                var loadedNoop = await _repository.GetForAssignmentAsync(requestId, ct);
                return (TechnicianMappings.ToAssignTechnicianResponse(loadedNoop!, language), "Technician_AlreadyAssigned");
            }

            await _uow.BeginTransactionAsync(ct);
            try
            {
                await _reqTechRepo.AddActiveAsync(requestId, technicianUserId, ct);

                if (request.CaseType == CaseType.Submitted)
                    request.CaseType = CaseType.Processing;

                request.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveAndCommitAsync(ct);

                var loaded = await _repository.GetForAssignmentAsync(requestId, ct);
                return (TechnicianMappings.ToAssignTechnicianResponse(loaded!, language), "Technician_Assigned");
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<(bool ok, string messageKey)> RemoveTechnicianAsync(
    int requestId,
    string technicianUserId,
    CancellationToken ct = default)
        {
            // 1) تأكد الطلب موجود
            var r = await _repository.GetForUpdateAsync(requestId, ct);
            if (r is null) return (false, "Request_NotFound");

            // 2) تأكد أن الفني مُعيَّن نشطًا أصلًا
            var isActive = await _reqTechRepo.IsActiveAssignedAsync(requestId, technicianUserId, ct);
            if (!isActive) return (false, "Technician_NotActiveOnRequest");

            await _uow.BeginTransactionAsync(ct);
            try
            {
                // 3) شطب التعيين النشط
                await _reqTechRepo.RemoveActiveAsync(requestId, technicianUserId, ct);

                // 4) إيقاف أي مؤقت نشط لهذا الفني على هذا الطلب
                await _workRepo.StopActiveForRequestAndTechAsync(requestId, technicianUserId, ct);

                // 5) تحديث طابع الوقت
                r.UpdatedAt = DateTime.UtcNow;

                // 6) حفظ + Commit
                await _uow.SaveAndCommitAsync(ct);

                return (true, "Technician_Removed");
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<(bool ok, string messageKey)> StartWorkAsync(
    int requestId,
    string technicianUserId,
    string callerUserId,
    string callerRole,
    CancellationToken ct = default)
{
    // 1) تحقق الطلب
    var req = await _repository.GetForUpdateAsync(requestId, ct);
    if (req is null) return (false, "Request_NotFound");

    // 2) الصلاحيات:
    // - الفني: لازم يبدأ لنفسه (callerUserId == technicianUserId)
    // - المدير: يقدر يبدأ لأي فني مُعيَّن نشطًا
    var isManager = callerRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
    var isTech = callerRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
    var callerIsSameTech = string.Equals(callerUserId, technicianUserId, StringComparison.Ordinal);

    if (!(isManager || (isTech && callerIsSameTech)))
        return (false, "Forbidden");

    // 3) تأكد أن الفني المطلوب مُعيَّن نشطًا على الطلب
    var isAssignedActive = await _reqTechRepo.IsActiveAssignedAsync(requestId, technicianUserId, ct);
    if (!isAssignedActive) return (false, "Request_NotAssignedToThisTechnician");

    // 4) منع الازدواج
    var hasActive = await _workRepo.HasActiveAsync(requestId, technicianUserId, ct);
    if (hasActive) return (false, "Work_AlreadyStarted");

    // 5) ابدأ ضمن ترانزاكشن
    await _uow.BeginTransactionAsync(ct);
    try
    {
        await _workRepo.StartAsync(new WorkTimeEntry
        {
            RequestId = requestId,
            TechnicianUserId = technicianUserId,
            StartedAt = DateTimeOffset.UtcNow
        }, ct);

        await _uow.SaveAndCommitAsync(ct);
        return (true, "Work_Started");
    }
    catch
    {
        await _uow.RollbackAsync(ct);
        throw;
    }
}

        public async Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> ChangeCaseAsync(
     int requestId,
     ChangeCaseTypeRequestDTO dto,
     string userId,
     string userRole,
     bool preferOwnerPath,
     string language = "ar",
     CancellationToken ct = default)
        {
            // تحقّقات بدون ترانزاكشن
            var r = await _repository.GetForUpdateAsync(requestId, ct);
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
                var respNoChange = MaintenanceRequestMapper.ToResponse(r, userRole, _fileService.GetPublicUrl, language, isOwner);
                return (respNoChange, "Case_NoChange");
            }

            if (newCase is CaseType.Submitted or CaseType.Modified)
                return (null, "Case_AutoManaged");

            bool needNote = newCase is CaseType.Reopened or CaseType.ResourcesNeeded;
            if (needNote && string.IsNullOrWhiteSpace(dto.NoteText))
                return (null, "Note_Required_For_This_Case");

            if (newCase == CaseType.Reopened && !dto.Priority.HasValue)
                return (null, "Priority_Required_For_Reopen");

            var inferredType = newCase switch
            {
                CaseType.Reopened => NoteType.ReopenReason,
                CaseType.ResourcesNeeded => NoteType.HelpRequest,
                _ => dto.NoteType ?? NoteType.General
            };


            if (useOwnerPath)
            {
                var allowedOwner = new[] { CaseType.Cancelled, CaseType.Reopened, CaseType.Completed };
                if (!allowedOwner.Contains(newCase))
                    return (null, "Case_NotAllowedForOwner");
            }
            else if (isTechnician)
            {
                var isActiveAssigned = await _reqTechRepo.IsActiveAssignedAsync(requestId, userId, ct);
                if (!isActiveAssigned)
                    return (null, "Request_NotAssignedToYou");

                var allowedTech = new[] { CaseType.ResourcesNeeded, CaseType.ManagerReview };
                if (!allowedTech.Contains(newCase))
                    return (null, "Case_NotAllowedForTechnician");
            }
            else if (isManager || isAdmin)
            {
                var allowedMgr = new[] { CaseType.ResourcesNeeded, CaseType.Processing, CaseType.Processed, CaseType.Completed };
                if (!allowedMgr.Contains(newCase))
                    return (null, "Case_NotAllowedForManager");
            }
            else
            {
                return (null, "Forbidden");
            }

            await _uow.BeginTransactionAsync(ct);
            try
            {
                if (useOwnerPath)
                {
                    r.CaseType = newCase;
                    if (newCase == CaseType.Reopened && dto.Priority.HasValue)
                        r.Priority = dto.Priority.Value;
                    if (needNote || !string.IsNullOrWhiteSpace(dto.NoteText))
                        r.Notes.Add(MaintenanceRequestMapper.ToNote(dto.NoteText!, inferredType, author, userId, r.Id));

                    r.UpdatedAt = DateTime.UtcNow;
                    await _workRepo.StopActiveForRequestAsync(requestId, ct);
                    await _uow.SaveAndCommitAsync(ct);

                    var resp = MaintenanceRequestMapper.ToResponse(r, userRole, _fileService.GetPublicUrl, language, isOwner);
                    return (resp, "Case_Changed");
                }

                if (isTechnician)
                {
                    r.CaseType = newCase;
                    if (needNote || !string.IsNullOrWhiteSpace(dto.NoteText))
                        r.Notes.Add(MaintenanceRequestMapper.ToNote(dto.NoteText!, inferredType, author, userId, r.Id));

                    r.UpdatedAt = DateTime.UtcNow;
                    await _workRepo.StopActiveForRequestAsync(requestId, ct);
                    await _uow.SaveAndCommitAsync(ct);

                    var resp = MaintenanceRequestMapper.ToResponse(r, userRole, _fileService.GetPublicUrl, language, isOwner);
                    return (resp, "Case_Changed");
                }

                // isManager || isAdmin
                {
                    r.CaseType = newCase;
                    if (needNote || !string.IsNullOrWhiteSpace(dto.NoteText))
                        r.Notes.Add(MaintenanceRequestMapper.ToNote(dto.NoteText!, inferredType, author, userId, r.Id));

                    r.UpdatedAt = DateTime.UtcNow;

                    if (newCase == CaseType.Processing)
                    {
                        var techs = await _reqTechRepo.GetActiveTechniciansAsync(requestId, ct);
                        if (techs.Count == 0)
                        {
                            await _uow.RollbackAsync(ct);
                            return (null, "Technician_NotAssigned");
                        }

                        foreach (var tid in techs)
                        {
                            var hasActive = await _workRepo.HasActiveAsync(requestId, tid, ct);
                            if (!hasActive)
                            {
                                await _workRepo.StartAsync(new WorkTimeEntry
                                {
                                    RequestId = requestId,
                                    TechnicianUserId = tid,
                                    StartedAt = DateTimeOffset.UtcNow
                                }, ct);
                            }
                        }
                    }
                    else
                    {
                        await _workRepo.StopActiveForRequestAsync(requestId, ct);

                        if (newCase == CaseType.Completed || newCase == CaseType.Cancelled)
                        {
                            var activeTechs = await _reqTechRepo.GetActiveTechniciansAsync(requestId, ct);
                            foreach (var tid in activeTechs)
                                await _reqTechRepo.RemoveActiveAsync(requestId, tid, ct);
                        }
                    }

                    await _uow.SaveAndCommitAsync(ct);

                    var resp = MaintenanceRequestMapper.ToResponse(r, userRole, _fileService.GetPublicUrl, language, isOwner);
                    return (resp, "Case_Changed");
                }
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> AddNoteAsync(
            int requestId,
            string userId,
            string userRole,
            AddNoteRequestDTO dto,
            string language = "ar",
            CancellationToken ct = default)
        {
            // تحقّقات بدون ترانزاكشن
            var r = await _repository.GetForUpdateAsync(requestId, ct);
            if (r is null) return (null, "Request_NotFound");

            var isOwner = string.Equals(r.CreatedByUserId, userId, StringComparison.Ordinal);
            var isTech = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            var isMgr = userRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            var author = InferAuthor(isOwner, isTech, isMgr, isAdmin);

            var lockedForManager = r.CaseType == CaseType.Completed || r.CaseType == CaseType.Cancelled;
            if (isMgr && lockedForManager && !isAdmin)
                return (null, "Notes_Disabled_For_Manager_In_FinalState");

            if (isTech)
            {
                var activeAssigned = await _reqTechRepo.IsActiveAssignedAsync(requestId, userId, ct); // NEW
                if (!activeAssigned) return (null, "Request_NotAssignedToYou");
            }

            if (!isOwner && !isTech && !isMgr && !isAdmin)
                return (null, "Forbidden");

            if (string.IsNullOrWhiteSpace(dto.Text))
                return (null, "Note_Text_Required");

            var noteType = dto.Type ?? NoteType.General;

            await _uow.BeginTransactionAsync(ct);
            try
            {
                r.Notes.Add(MaintenanceRequestMapper.ToNote(dto.Text!, noteType, author, userId, r.Id));
                r.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveAndCommitAsync(ct);

                return (MaintenanceRequestMapper.ToResponse(r, userRole, _fileService.GetPublicUrl, language, isOwner),
                        "Note_Added");
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)>
            UpdateMineAsync(
                int id,
                string userId,
                string role,
                MaintenanceRequestUpdateDTO dto,
                string language = "ar",
                CancellationToken ct = default)
        {
            // 0) جهّز عمليات الملفات
            var uploadedNewFiles = new List<string>();   // نعوّضها لو فشلنا
            var toDeleteFiles = new List<string>();      // نحذفها بعد الـ Commit

            // 1) ارفع الصور الجديدة خارج الترانزاكشن (تعويض عند الفشل)
            if (dto.NewImages != null && dto.NewImages.Count > 0)
            {
                foreach (var f in dto.NewImages)
                {
                    if (f is { Length: > 0 })
                    {
                        var name = await _fileService.UploadAsync(f, ct);
                        uploadedNewFiles.Add(name);
                    }
                }
            }

            await _uow.BeginTransactionAsync(ct);
            try
            {
                var r = await _repository.GetForUpdateAsync(id, ct);
                if (r is null) return (null, "Request_NotFound");

                // المالك فقط
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

                // 2) تعديل الحقول النصية
                if (!string.IsNullOrWhiteSpace(dto.Title)) r.Title = dto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Description)) r.Description = dto.Description.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Address)) r.Address = dto.Address.Trim();
                if (dto.Priority.HasValue) r.Priority = dto.Priority.Value;
                if (dto.ProblemTypeId.HasValue) r.ProblemTypeId = dto.ProblemTypeId.Value;

                // 3) حذف صور (من الداتابيس الآن، ومن التخزين بعد الـ Commit)
                if (dto.RemoveImageIds is { Count: > 0 } && r.Images.Count > 0)
                {
                    var toRemove = r.Images.Where(i => dto.RemoveImageIds.Contains(i.Id)).ToList();
                    foreach (var img in toRemove)
                    {
                        toDeleteFiles.Add(img.FileName); // احذف فعليًا بعد الـ Commit
                        r.Images.Remove(img);
                    }
                }

                // 4) إضافة الصور المرفوعة حديثًا إلى الداتابيس
                foreach (var name in uploadedNewFiles)
                {
                    r.Images.Add(new MaintenanceRequestImage
                    {
                        FileName = name,
                        IsPrimary = false
                    });
                }

                // 5) ضمان وجود صورة أساسية واحدة
                if (r.Images.Count > 0 && !r.Images.Any(i => i.IsPrimary))
                    r.Images.First().IsPrimary = true;

                // 6) انتقال الحالة وتحديث الوقت
                r.CaseType = CaseType.Modified;
                r.UpdatedAt = DateTime.UtcNow;

                // 7) حفظ + Commit مرّة واحدة
                await _uow.SaveAndCommitAsync(ct);

                // 8) بعد الـ Commit: نفّذ الحذف الفعلي للملفات التي أزلنا روابطها
                foreach (var filename in toDeleteFiles)
                {
                    try { await _fileService.DeleteAsync(filename, ct); } catch { /* تجاهل */ }
                }

                var isOwner = true; // مؤكّد من الفحص أعلاه
                var response = MaintenanceRequestMapper.ToResponse(r, role, _fileService.GetPublicUrl, language, isOwner);
                return (response, "Request_Updated");
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                foreach (var name in uploadedNewFiles)
                {
                    try { await _fileService.DeleteAsync(name, ct); } catch { /* ولا يهمك */ }
                }
                throw;
            }
        }

        private static NoteAuthor InferAuthor(bool isOwner, bool isTech, bool isMgr, bool isAdmin)
        {
            if (isAdmin) return NoteAuthor.Admin;
            if (isMgr) return NoteAuthor.Manager;
            if (isTech) return NoteAuthor.Technician;
            return NoteAuthor.Owner;
        }

        public async Task<(MaintenanceRequestResponseDTO? Response, string MessageKey)> AddImagesAsync(
    int requestId,
    string userId,
    string userRole,
    AddImagesRequestDTO dto,
    string language = "ar",
    CancellationToken ct = default)
        {
            // 1) تحقّقات أولية
            var r = await _repository.GetForUpdateAsync(requestId, ct);
            if (r is null) return (null, "Request_NotFound");

            var isOwner = string.Equals(r.CreatedByUserId, userId, StringComparison.Ordinal);
            var isTech = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
            var isMgr = userRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
            var isAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            // نمنع إضافة صور بعد الإنهاء/الإلغاء (يسمح فقط للأدمن لو أردت)
            var lockedFinal = r.CaseType == CaseType.Completed || r.CaseType == CaseType.Cancelled;
            if (lockedFinal && !isAdmin)
                return (null, "Images_Disabled_In_FinalState");

            // الفني يجب أن يكون مُعيَّن تعيينًا نشطًا
            if (isTech)
            {
                var activeAssigned = await _reqTechRepo.IsActiveAssignedAsync(requestId, userId, ct);
                if (!activeAssigned) return (null, "Request_NotAssignedToYou");
            }

            // السماح: صاحب الطلب، الفني (لو مُعيّن)، المدير، الأدمن
            if (!isOwner && !isTech && !isMgr && !isAdmin)
                return (null, "Forbidden");

            // 2) تحقق من وجود ملفات
            if (dto?.Images is null || dto.Images.Count == 0)
                return (null, "Images_Empty");

            // (اختياري) تحقق النوع/الحجم
            // مثال بسيط: رفض > 10MB للصورة الواحدة
            const long maxSize = 10 * 1024 * 1024;
            foreach (var f in dto.Images)
            {
                if (f == null || f.Length <= 0) return (null, "Images_InvalidFile");
                if (f.Length > maxSize) return (null, "Images_TooLarge");
                // بإمكانك فحص ContentType إن رغبت: image/jpeg, image/png ...
            }

            // 3) ارفع وألصق
            var uploaded = new List<string>();

            await _uow.BeginTransactionAsync(ct);
            try
            {
                // ارفع أولًا
                foreach (var file in dto.Images)
                    uploaded.Add(await _fileService.UploadAsync(file, ct));

                // اربط بالطلب
                var hadPrimaryBefore = r.Images?.Any(i => i.IsPrimary) == true;
                for (int i = 0; i < uploaded.Count; i++)
                {
                    var isPrimary = false;
                    if (!hadPrimaryBefore && dto.MakePrimaryFirst && i == 0)
                        isPrimary = true;

                    r.Images.Add(new MaintenanceRequestImage
                    {
                        FileName = uploaded[i],
                        IsPrimary = isPrimary
                    });
                }

                r.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveAndCommitAsync(ct);
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                // نظّف الصور المرفوعة إذا فشل الحفظ
                foreach (var name in uploaded)
                {
                    try { await _fileService.DeleteAsync(name, ct); } catch { }
                }
                throw;
            }

            // 4) رجّع الردّ مع الروابط العامة
            var withIncludes = await _repository.Query(
                    asTracking: false,
                    include: q => q
                        .Include(x => x.Images)
                        .Include(x => x.Notes)
                        .Include(x => x.ProblemType).ThenInclude(pt => pt.Translations),
                    predicate: x => x.Id == requestId)
                .FirstOrDefaultAsync(ct);

            var dtoRes = withIncludes is null
                ? null
                : MaintenanceRequestMapper.ToResponse(withIncludes, userRole, _fileService.GetPublicUrl, language,
                      isOwner: string.Equals(withIncludes.CreatedByUserId, userId, StringComparison.Ordinal));

            return (dtoRes, "Images_Added");
        }

    }
}





