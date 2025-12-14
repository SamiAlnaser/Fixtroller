using Fixtroller.BLL.Mapping;
using Fixtroller.BLL.Services.FileService;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.PagedResultDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.UsersDTOS.Responses;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.MaintenanceRequestRepositories;
using Fixtroller.DAL.Repositories.NumbersRepositories;
using Fixtroller.DAL.Repositories.UserRepository.TechnicianRepositorirs;
using Fixtroller.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.TechnicianServices
{

    public class TechnicianService : ITechnicianService
    {
        private readonly ITechnicianRepository _repository;
        private readonly IMaintenanceRequestRepository _reqRepo;
        private readonly IFileService _fileService;
        private readonly IUnitOfWork _uow;
        private readonly IMetricsRepository _metricsrepo;

        public TechnicianService(
            ITechnicianRepository repository,
            IMaintenanceRequestRepository reqRepo,
            IFileService fileService,
            IUnitOfWork uow,
          IMetricsRepository metricsrepo)
        {
            _repository = repository;
            _reqRepo = reqRepo;
            _fileService = fileService;
            _uow = uow;
            _metricsrepo = metricsrepo;
        }


        public async Task<PagedResultDTO<TechnicianListItemDTO>> GetWithMetricsAsync(
      string language,
      int? technicianCategoryId,
      string? search,
      int pageNumber = 1,
      int pageSize = 10,
      CancellationToken ct = default)
        {
            language = string.IsNullOrWhiteSpace(language) ? "ar" : language;

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var techs = await _repository.GetAsync(technicianCategoryId, search, ct);
            var techIds = techs.Select(t => t.Id).ToList();

            if (techIds.Count == 0)
            {
                return new PagedResultDTO<TechnicianListItemDTO>
                {
                    TotalPages = 0,
                    CurrentPage = pageNumber,
                    Data = new List<TechnicianListItemDTO>()
                };
            }

            var reqStatsTuples = await _metricsrepo.GetRequestStatsPerTechnicianAsync(techIds, ct);
            var avgTuples = await _metricsrepo.GetAvgCompletionMinutesPerTechnicianAsync(techIds, ct);

            var reqDict = reqStatsTuples.ToDictionary(
                x => x.TechnicianUserId,
                x => (x.AssignedCount, x.CompletedCount));

            var avgDict = avgTuples.ToDictionary(
                x => x.TechnicianUserId,
                x => x.AvgCompletionMinutes);

            var list = techs.Select(t =>
            {
                var catName = t.TechnicianCategory?.Translations?
                    .OrderBy(tr => tr.Language == language ? 0 : tr.Language == "ar" ? 1 : 2)
                    .Select(tr => tr.Name)
                    .FirstOrDefault();

                reqDict.TryGetValue(t.Id, out var stats);
                var assigned = stats.AssignedCount;
                var completed = stats.CompletedCount;

                var avg = avgDict.TryGetValue(t.Id, out var m) ? m : 0;

                return new TechnicianListItemDTO
                {
                    TechnicianUserId = t.Id,
                    TechnicianName = t.FullName ?? string.Empty,
                    TechnicianCategory = catName,
                    AssignedCount = assigned,
                    CompletedCount = completed,
                    AvgCompletionMinutes = avg
                };
            })
            .OrderBy(x => x.TechnicianName)
            .ToList();

            // 👇 هنا فقط الباجينيشن
            var totalCount = list.Count;
            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            var pageData = list
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResultDTO<TechnicianListItemDTO>
            {
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                Data = pageData
            };
        }



        public async Task<bool> UpdateTechnicianCategoryAsync(
            UpdateTechnicianCategoryRequestDTO dto,
            CancellationToken ct = default)
        {
            // تأكد أنه فعلاً Technician
            var isTech = await _repository.IsInRoleAsync(dto.TechnicianUserId, "Technician", ct);
            if (!isTech) return false;

            // عدّل فقط… الحفظ عبر UoW
            var ok = await _repository.UpdateCategoryAsync(dto.TechnicianUserId, dto.TechnicianCategoryId, ct);
            if (!ok) return false;

            await _uow.SaveAndCommitAsync(ct);
            return true;
        }

        public async Task<bool> ClearTechnicianCategoryAsync(
          ClearTechnicianCategoryRequestDTO dto,
          CancellationToken ct = default)
        {
            // تأكد أنه فعلاً Technician
            var isTech = await _repository.IsInRoleAsync(dto.TechnicianUserId, "Technician", ct);
            if (!isTech) return false;

            var ok = await _repository.ClearCategoryAsync(dto.TechnicianUserId, ct);
            if (!ok) return false;

            await _uow.SaveAndCommitAsync(ct);
            return true;
        }

        public async Task<PagedResultDTO<TechnicianBoardDTO>> GetMyAssignedAsync(
      string technicianUserId,
      string language,
      int pageNumber = 1,
      int pageSize = 10,
      DateTime? createdFrom = null,
      DateTime? createdTo = null,
      int? requestId = null,
      CancellationToken ct = default)
        {
            language = string.IsNullOrWhiteSpace(language) ? "ar" : language;

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            // ✅ Normalize + تأمين نطاق التاريخ
            static DateTime NormalizeEnd(DateTime d) =>
                d.TimeOfDay == TimeSpan.Zero ? d.Date.AddDays(1).AddTicks(-1) : d;

            if (createdFrom.HasValue && createdTo.HasValue && createdFrom.Value > createdTo.Value)
                (createdFrom, createdTo) = (createdTo, createdFrom);

            var end = createdTo.HasValue ? NormalizeEnd(createdTo.Value) : (DateTime?)null;

            // 1) الكويري الأساسي + Include لنوع المشكلة وترجماته
            IQueryable<MaintenanceRequest> q = _reqRepo.Query(
                    asTracking: false,
                    predicate: x =>
                        x.Status == Status.Active &&
                        x.Technicians.Any(t =>
                            t.TechnicianUserId == technicianUserId &&
                            t.UnassignedAtUtc == null))
                .Include(x => x.ProblemType)
                    .ThenInclude(pt => pt.Translations);

            if (requestId.HasValue && requestId.Value > 0)
                q = q.Where(x => x.Id == requestId.Value);

            // ✅ فلترة التاريخ (CreatedAt)
            if (createdFrom.HasValue)
                q = q.Where(x => x.CreatedAt >= createdFrom.Value);

            if (end.HasValue)
                q = q.Where(x => x.CreatedAt <= end.Value);

            // 2) إجمالي عدد الطلبات المسندة (لأجل عدد الصفحات)
            var totalCount = await q.CountAsync(ct);
            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            // 3) نسحب حقول خفيفة + اسم نوع المشكلة مترجَم
            var rows = await q
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    Light = new MaintenanceRequest
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Description = x.Description,
                        CaseType = x.CaseType,
                        Priority = x.Priority,
                        CreatedAt = x.CreatedAt
                    },

                    ProblemTypeName = x.ProblemType != null
                        ? x.ProblemType.Translations
                            .OrderBy(t =>
                                t.Language == language ? 0 :
                                t.Language == "ar" ? 1 : 2)
                            .Select(t => t.Name)
                            .FirstOrDefault()
                        : null
                })
                .ToListAsync(ct);

            // 4) تصنيف الأعمدة حسب حالة الطلب
            static bool IsNew(CaseType c) =>
                c == CaseType.Submitted || c == CaseType.ManagerReview;

            static bool IsInProgress(CaseType c) =>
                c == CaseType.Processing
                || c == CaseType.ResourcesNeeded
                || c == CaseType.Modified
                || c == CaseType.Reopened
                || c == CaseType.Processed;

            static bool IsCompleted(CaseType c) => c == CaseType.Completed;

            var newAll = rows
                .Where(r => IsNew(r.Light.CaseType))
                .OrderByDescending(r => r.Light.CreatedAt)
                .ToList();

            var progressAll = rows
                .Where(r => IsInProgress(r.Light.CaseType))
                .OrderByDescending(r => r.Light.CreatedAt)
                .ToList();

            var completedAll = rows
                .Where(r => IsCompleted(r.Light.CaseType))
                .OrderByDescending(r => r.Light.CreatedAt)
                .ToList();

            // 5) تجهيز البورد مع تمرير ProblemTypeName للمابر
            var board = new TechnicianBoardDTO
            {
                New = new TechnicianBoardColumnDTO
                {
                    Title = language == "ar" ? "مهام جديدة" : "New Tasks",
                    Count = newAll.Count,
                    Items = newAll
                        .Select(r => MaintenanceRequestMapper.ToTechnicianCard(
                            r.Light,
                            language,
                            r.ProblemTypeName))
                        .ToList()
                },
                InProgress = new TechnicianBoardColumnDTO
                {
                    Title = language == "ar" ? "قيد التنفيذ" : "In Progress",
                    Count = progressAll.Count,
                    Items = progressAll
                        .Select(r => MaintenanceRequestMapper.ToTechnicianCard(
                            r.Light,
                            language,
                            r.ProblemTypeName))
                        .ToList()
                },
                Completed = new TechnicianBoardColumnDTO
                {
                    Title = language == "ar" ? "مكتملة" : "Completed",
                    Count = completedAll.Count,
                    Items = completedAll
                        .Select(r => MaintenanceRequestMapper.ToTechnicianCard(
                            r.Light,
                            language,
                            r.ProblemTypeName))
                        .ToList()
                }
            };

            // 6) إرجاع النتيجة في PagedResultDTO (Data فيها عنصر واحد: البورد)
            return new PagedResultDTO<TechnicianBoardDTO>
            {
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                TotalCount = totalCount,
                PageSize = pageSize,
                Data = new List<TechnicianBoardDTO> { board }
            };
        }

        public async Task<PagedResultDTO<TechnicianResponseDTO>> GetByCategoryAsync(
    int categoryId,
    string? search,
    string language,
    int pageNumber = 1,
    int pageSize = 10,
    CancellationToken ct = default)
        {
            language = string.IsNullOrWhiteSpace(language) ? "ar" : language;

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var users = await _repository.GetByCategoryAsync(categoryId, search, ct);
            var list = users
                .Select(u => TechnicianMappings.ToTechnicianResponse(u, language))
                .ToList();

            var totalCount = list.Count;
            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            var pageData = list
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResultDTO<TechnicianResponseDTO>
            {
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                Data = pageData
            };
        }



    }
}
