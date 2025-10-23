using Fixtroller.BLL.Mapping;
using Fixtroller.BLL.Services.FileService;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses;
using Fixtroller.DAL.Repositories.MaintenanceRequestepository;
using Fixtroller.DAL.Repositories.UserRepository.TechnicianRepositorirs;
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

        public TechnicianService(ITechnicianRepository repository , IMaintenanceRequestRepository reqRepo, IFileService fileService)
        {
            _repository = repository;
            _reqRepo = reqRepo;
            _fileService = fileService;
        }

        public async Task<IReadOnlyList<TechnicianResponseDTO>> GetAsync(TechniciansFilterRequestDTO filter)
        {
            var users = await _repository.GetAsync(filter.TechnicianCategoryId, filter.Search);
            var list = users.Select(u => TechnicianMappings.ToTechnicianResponse(u, filter.Language)).ToList();
            return list.AsReadOnly();
        }
        public async Task<bool> UpdateTechnicianCategoryAsync(UpdateTechnicianCategoryRequestDTO dto)
        {
      
            var isTech = await _repository.IsInRoleAsync(dto.TechnicianUserId, "Technician");
            if (!isTech) return false;

            return await _repository.UpdateCategoryAsync(dto.TechnicianUserId, dto.TechnicianCategoryId);
        }
        public async Task<IReadOnlyList<TechnicianAssignedRequestResponseDTO>> GetMyAssignedAsync(
            string technicianUserId, string language = "ar")
        {
            var list = await _reqRepo
                .QueryAssignedTo(technicianUserId, asTracking: false)
                .Include(r => r.Images)
                .Include(r => r.ProblemType).ThenInclude(pt => pt.Translations)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return list
                .Select(r => TechnicianMappings.ToTechnicianAssigned(r, language, _fileService.GetPublicUrl))
                .ToList()
                .AsReadOnly();
        }
        public async Task<IReadOnlyList<TechnicianResponseDTO>> GetByCategoryAsync(int categoryId, string? search, string language)
        {
            var users = await _repository.GetByCategoryAsync(categoryId, search);
            var list = users.Select(u => TechnicianMappings.ToTechnicianResponse(u, language)).ToList();
            return list.AsReadOnly();
        }
    }
}
