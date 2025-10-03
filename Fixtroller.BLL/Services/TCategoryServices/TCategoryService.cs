using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.Data.DTOs.ProblemTypeDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Responses;
using Fixtroller.DAL.DTOs.TCategoryDTOs.Requests;
using Fixtroller.DAL.DTOs.TCategoryDTOs.Responses;
using Fixtroller.DAL.Entities.TechnicianCategoryEntity;
using Fixtroller.DAL.Repositories.TCategoryRepositories;
using Mapster;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.TCategoryServices
{
    public class TCategoryService : GenericService<TCategoryRequestDTO, TCategoryResponseDTO, TechnicianCategory> , ITCategoryService
    {
        private readonly ITCategoryRepository _repository;

        public TCategoryService(ITCategoryRepository repository) : base(repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TCategoryUserResponseDTO>> GetActiveForUserAsync()
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var list = await _repository.GetActiveForUserAsync();

            return list.Select(e => new TCategoryUserResponseDTO
            {
                Id = e.Id,
                Name = e.Translations
                        .FirstOrDefault(t => t.Language == lang)?.Name
                     ?? e.Translations
                        .FirstOrDefault(t => t.Language == "ar")?.Name
            });
        }

        public async Task<IEnumerable<TCategoryUserResponseDTO>> GetAllForUserAsync()
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var list = await _repository.GetAllForUserAsync();


            return list.Select(e => new TCategoryUserResponseDTO
            {
                Id = e.Id,
                Name = e.Translations
                            .FirstOrDefault(t => t.Language == lang)?.Name
                         ?? e.Translations
                            .FirstOrDefault(t => t.Language == "ar")?.Name
            });
        }

        public async Task<TCategoryUserResponseDTO?> GetByIdForUserAsync(int id)
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var e = await _repository.GetByIdForUserAsync(id);
            if (e is null) return null;

            return new TCategoryUserResponseDTO
            {
                Id = e.Id,
                Name = e.Translations
                     .FirstOrDefault(t => t.Language == lang)?.Name
                  ?? e.Translations
                     .FirstOrDefault(t => t.Language == "ar")?.Name
            };
        }
    }
}
