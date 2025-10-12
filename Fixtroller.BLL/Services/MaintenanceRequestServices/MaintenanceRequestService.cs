using Fixtroller.BLL.Mapping;
using Fixtroller.BLL.Services.FileService;
using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.MaintenanceRequestepository;
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
        private readonly IFileService _fileService;
        public MaintenanceRequestService(IMaintenanceRequestRepository repository, IFileService fileService) : base(repository)
        {
            _repository = repository;
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
    }
}
    

