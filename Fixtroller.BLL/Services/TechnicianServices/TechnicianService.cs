using Fixtroller.BLL.Mapping;
using Fixtroller.BLL.Services.FileService;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using Fixtroller.DAL.Repositories.MaintenanceRequestepository;
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

        public TechnicianService(
            ITechnicianRepository repository,
            IMaintenanceRequestRepository reqRepo,
            IFileService fileService,
            IUnitOfWork uow)
        {
            _repository = repository;
            _reqRepo = reqRepo;
            _fileService = fileService;
            _uow = uow;
        }

        public async Task<IReadOnlyList<TechnicianResponseDTO>> GetAsync(
            TechniciansFilterRequestDTO filter,
            CancellationToken ct = default)
        {
            var users = await _repository.GetAsync(filter.TechnicianCategoryId, filter.Search, ct);
            var list = users.Select(u => TechnicianMappings.ToTechnicianResponse(u, filter.Language)).ToList();
            return list.AsReadOnly();
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

        public async Task<IReadOnlyList<TechnicianAssignedRequestResponseDTO>> GetMyAssignedAsync(
            string technicianUserId,
            string language = "ar",
            CancellationToken ct = default)
        {
            var list = await _reqRepo
                .QueryAssignedTo(technicianUserId, asTracking: false)
                .Include(r => r.Images)
                .Include(r => r.ProblemType).ThenInclude(pt => pt.Translations)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);

            return list
                .Select(r => TechnicianMappings.ToTechnicianAssigned(r, language, _fileService.GetPublicUrl))
                .ToList()
                .AsReadOnly();
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
