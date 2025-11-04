using Fixtroller.BLL.Mapping;
using Fixtroller.BLL.Services.FileService;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.MaintenanceRequestepository;
using Fixtroller.DAL.Repositories.NumbersRepository;
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


        public async Task<IEnumerable<TechnicianListItemDTO>> GetWithMetricsAsync(
    string language, int? technicianCategoryId, string? search, CancellationToken ct = default)
        {
            language = string.IsNullOrWhiteSpace(language) ? "ar" : language;

            var techs = await _repository.GetAsync(technicianCategoryId, search, ct);
            var techIds = techs.Select(t => t.Id).ToList();
            if (techIds.Count == 0) return Array.Empty<TechnicianListItemDTO>();

            var reqStatsTuples = await _metricsrepo.GetRequestStatsPerTechnicianAsync(techIds, ct);
            var avgTuples = await _metricsrepo.GetAvgCompletionMinutesPerTechnicianAsync(techIds, ct);

            var reqDict = reqStatsTuples.ToDictionary(x => x.TechnicianUserId, x => (x.AssignedCount, x.CompletedCount));
            var avgDict = avgTuples.ToDictionary(x => x.TechnicianUserId, x => x.AvgCompletionMinutes);

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

            return list;
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

        public async Task<TechnicianBoardDTO> GetMyAssignedAsync(
            string technicianUserId,
            string language,
            CancellationToken ct = default)
        {
            var q = _reqRepo.Query(
                asTracking: false,
                predicate: x => x.Status == Status.Active &&
                                x.Technicians.Any(t =>
                                    t.TechnicianUserId == technicianUserId &&
                                    t.UnassignedAtUtc == null));

            // نسحب حقول خفيفة فقط
            var rows = await q
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new MaintenanceRequest
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    CaseType = x.CaseType,
                    Priority = x.Priority,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(ct);

            // تصنيف الأعمدة
            static bool IsNew(CaseType c) =>
                c == CaseType.Submitted || c == CaseType.ManagerReview;

            static bool IsInProgress(CaseType c) => c == CaseType.Processing
                || c == CaseType.ResourcesNeeded
                || c == CaseType.Modified
                || c == CaseType.Reopened
                || c == CaseType.Processed;

            static bool IsCompleted(CaseType c) => c == CaseType.Completed;

            var newAll = rows.Where(r => IsNew(r.CaseType)).OrderByDescending(r => r.CreatedAt).ToList();
            var progressAll = rows.Where(r => IsInProgress(r.CaseType)).OrderByDescending(r => r.CreatedAt).ToList();
            var completedAll = rows.Where(r => IsCompleted(r.CaseType)).OrderByDescending(r => r.CreatedAt).ToList();

            return new TechnicianBoardDTO
            {
                New = new TechnicianBoardColumnDTO
                {
                    Title = language == "ar" ? "مهام جديدة" : "New Tasks",
                    Count = newAll.Count,
                    Items = newAll.Select(r => MaintenanceRequestMapper.ToTechnicianCard(r, language)).ToList()
                },
                InProgress = new TechnicianBoardColumnDTO
                {
                    Title = language == "ar" ? "قيد التنفيذ" : "In Progress",
                    Count = progressAll.Count,
                    Items = progressAll.Select(r => MaintenanceRequestMapper.ToTechnicianCard(r, language)).ToList()
                },
                Completed = new TechnicianBoardColumnDTO
                {
                    Title = language == "ar" ? "مكتملة" : "Completed",
                    Count = completedAll.Count,
                    Items = completedAll.Select(r => MaintenanceRequestMapper.ToTechnicianCard(r, language)).ToList()
                }
            };
        }
        public async Task<IReadOnlyList<TechnicianResponseDTO>> GetByCategoryAsync(
            int categoryId,
            string? search,
            string language,
            CancellationToken ct = default)
        {
            var users = await _repository.GetByCategoryAsync(categoryId, search, ct);
            var list = users.Select(u => TechnicianMappings.ToTechnicianResponse(u, language)).ToList();
            return list.AsReadOnly();
        }
    }
}
