using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.Data.DTOs.ProblemTypeDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Requests;
using Fixtroller.DAL.Entities.TechnicianCategoryEntity;
using Fixtroller.DAL.Repositories.TCategoryRepositories;
using Fixtroller.DAL.UnitOfWork;
using Mapster;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.TCategoryServices
{
    public class TCategoryService
     : GenericService<TCategoryRequestDTO, TCategoryResponseDTO, TechnicianCategory>, ITCategoryService
    {
        private readonly ITCategoryRepository _repository;

        public TCategoryService(ITCategoryRepository repository, IUnitOfWork uow )
            : base(repository , uow)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TCategoryUserResponseDTO>> GetActiveForUserAsync(
            string language,
            CancellationToken ct = default)
        {
            var list = await _repository.GetActiveForUserAsync(asTracking: false, ct);

            return list.Select(e => new TCategoryUserResponseDTO
            {
                Id = e.Id,
                Name = e.Translations
                         .FirstOrDefault(t => t.Language == language)?.Name
                      ?? e.Translations
                         .FirstOrDefault(t => t.Language == "ar")?.Name
            });
        }

        public async Task<IEnumerable<TCategoryUserResponseDTO>> GetAllForUserAsync(
            string language,
            bool? isActive,
            CancellationToken ct = default)
        {
            var list = await _repository.GetAllForUserAsync(
                isActive: isActive,
                asTracking: false,
                ct: ct);

            return list.Select(e => new TCategoryUserResponseDTO
            {
                Id = e.Id,
                Name = e.Translations
                         .FirstOrDefault(t => t.Language == language)?.Name
                      ?? e.Translations
                         .FirstOrDefault(t => t.Language == "ar")?.Name
            });
        }


        public async Task<TCategoryUserResponseDTO?> GetByIdForUserAsync(
            int id,
            string language,
            CancellationToken ct = default)
        {
            var e = await _repository.GetByIdForUserAsync(id, ct);
            if (e is null) return null;

            return new TCategoryUserResponseDTO
            {
                Id = e.Id,
                Name = e.Translations
                         .FirstOrDefault(t => t.Language == language)?.Name
                      ?? e.Translations
                         .FirstOrDefault(t => t.Language == "ar")?.Name
            };
        }
    }
}

