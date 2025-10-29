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
        public MaintenanceRequestService(IMaintenanceRequestRepository repository, ITechnicianRepository techRepo, IFileService fileService,IUnitOfWork uow) : base(repository,  uow)
        {
            _repository = repository;
            _techRepo = techRepo;
            _fileService = fileService;
            _uow = uow;

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
                uploaded.Add(await _fileService.UploadAsync(f));
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
        // 2) أضف للـ DbContext بدون Save داخلي
        await _repository.AddAsync(entity);

        // 3) حفظ + Commit مرّة واحدة
        await _uow.SaveAndCommitAsync(ct);

        // EF يملأ الـ Id بعد الحفظ
        return entity.Id;
    }
    catch
    {
        // 4) Rollback + تعويض (حذف الملفات المرفوعة)
        await _uow.RollbackAsync(ct);
        foreach (var name in uploaded)
        {
            try { await _fileService.DeleteAsync(name); } catch { /* ولا همّك */ }
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
            // طبّق الصلاحيات كـ ثوابت تُمرَّر للـ EF (تُترجم كمعاملات)
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
                                (isTechnician && (x.AssignedTechnicianUserId == userId || x.CreatedByUserId == userId))
                            ),
                        include: q => q
                            .Include(r => r.Images)
                            .Include(r => r.Notes)
                            .Include(r => r.AssignedTechnician)
                                .ThenInclude(t => t.TechnicianCategory)
                                    .ThenInclude(c => c.Translations)
                    )
                    .FirstOrDefaultAsync(ct);

            if (e is null) return null;

            var isOwner = string.Equals(e.CreatedByUserId, userId, StringComparison.Ordinal);
            return MaintenanceRequestMapper.ToResponse(e, role, _fileService.GetPublicUrl, language, isOwner);
        }

        public async Task<(AssignTechnicianResponseDTO? Response, string MessageKey)> AssignTechnicianAsync(
     int requestId,
     string technicianUserId,
     string language = "ar",
     CancellationToken ct = default)
        {
            await _uow.BeginTransactionAsync(ct);

            try
            {
                // الطلب (بتتبّع/تراكنج لأننا سنعدّل عليه)
                var request = await _repository.GetForAssignmentAsync(requestId, ct);
                if (request is null)
                    return (null, "Request_NotFound");

                // الفني موجود؟
                var tech = await _techRepo.GetByIdAsync(technicianUserId, ct); // AsNoTracking OK
                if (tech is null)
                    return (null, "Technician_NotFound");

                // يتبع لدور Technician؟
                var isTechnician = await _techRepo.IsInRoleAsync(technicianUserId, "Technician", ct);
                if (!isTechnician)
                    return (null, "User_NotTechnician");

                // الإسناد + تهيئة الحالة
                request.AssignedTechnicianUserId = technicianUserId;
                request.AssignedAtUtc = DateTime.UtcNow;
                if (request.CaseType == CaseType.Submitted)
                    request.CaseType = CaseType.Processing;

                // حفظ + Commit مرّة واحدة
                await _uow.SaveAndCommitAsync(ct);

                // إعادة تحميل مع الـ Includes اللازمة للـ Mapping
                var loaded = await _repository.GetForAssignmentAsync(requestId, ct);

                return (TechnicianMappings.ToAssignTechnicianResponse(loaded!, language), "Technician_Assigned");
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
            await _uow.BeginTransactionAsync(ct);

            try
            {
                var r = await _repository.GetForUpdateAsync(requestId, ct);
                if (r is null) return (null, "Request_NotFound");

                var newCase = dto.NewCaseType;

                bool isManager = userRole.Equals("MaintenanceManager", StringComparison.OrdinalIgnoreCase);
                bool isAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
                bool isTechnician = userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase);
                bool isOwner = r.CreatedByUserId == userId;

                var useOwnerPath = isOwner && (preferOwnerPath || !(isManager || isAdmin || isTechnician));

                var author = InferAuthor(isOwner, isTechnician, isManager, isAdmin);

                // لا تغيير
                if (r.CaseType == newCase)
                {
                    var respNoChange = MaintenanceRequestMapper.ToResponse(
                        r, userRole, _fileService.GetPublicUrl, language, isOwner);
                    return (respNoChange, "Case_Changed");
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

                // مسار المالك
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

                    await _uow.SaveAndCommitAsync(ct);

                    var resp = MaintenanceRequestMapper.ToResponse(
                        r, userRole, _fileService.GetPublicUrl, language, isOwner);
                    return (resp, "Case_Changed");
                }

                // مسار الفني
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

                    await _uow.SaveAndCommitAsync(ct);

                    var resp = MaintenanceRequestMapper.ToResponse(
                        r, userRole, _fileService.GetPublicUrl, language, isOwner);
                    return (resp, "Case_Changed");
                }

                // مسار المدير أو الأدمن
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

                    await _uow.SaveAndCommitAsync(ct);

                    var resp = MaintenanceRequestMapper.ToResponse(
                        r, userRole, _fileService.GetPublicUrl, language, isOwner);
                    return (resp, "Case_Changed");
                }

                return (null, "Forbidden");
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
            await _uow.BeginTransactionAsync(ct);
            try
            {
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

                await _uow.SaveAndCommitAsync(ct);

                // رجّع الطلب مع الملاحظات
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
            var toDeleteFiles = new List<string>();   // نحذفها بعد الـ Commit

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
                // Rollback وتعويض: احذف أي ملفات جديدة رفعناها قبل الترانزاكشن
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


    }
}
    


    

