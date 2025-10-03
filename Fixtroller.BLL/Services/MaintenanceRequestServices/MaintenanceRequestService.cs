using Fixtroller.BLL.Services.FileService;
using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Responses;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.MaintenanceRequestepository;
using Mapster;
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
        public async Task<int> CreateWithFile(MaintenanceRequestRequestDTO request)
        {
            var entity = request.Adapt<MaintenanceRequest>();
            entity.CreatedAt = DateTime.UtcNow;

            if (request.MainImage != null)
            {
                var imagePath = await _fileService.UploadAsync(request.MainImage);
                entity.MainImage = imagePath;
            }

            return await _repository.AddAsync(entity);
        }
    }
}
