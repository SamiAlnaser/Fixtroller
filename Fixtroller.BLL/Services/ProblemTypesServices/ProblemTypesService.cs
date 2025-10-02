using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.Data.DTOs.ProblemTypeDTOs.Responses;
using Fixtroller.DAL.DTOs.ProblemTypeDTOs.Requests;
using Fixtroller.DAL.DTOs.ProblemTypeDTOs.Responses;
using Fixtroller.DAL.DTOs.TCategoryDTOs.Responses;
using Fixtroller.DAL.Entities.ProblemType;
using Fixtroller.DAL.Repositories.ProblemTypeRepositories;
using Mapster;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.ProblemTypesServices
{
    public class ProblemTypesService : GenericService<ProblemTypeRequestDTO, ProblemTypeResponseDTO, ProblemType> , IProblemTypesService
    {
        private readonly IProblemTypeRepository _repository;
        public ProblemTypesService(IProblemTypeRepository repository) : base(repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProblemTypeUserResponseDTO>> GetAllForUserAsync()
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var list = await _repository.GetAllForUserAsync();


            return list.Select(e => new ProblemTypeUserResponseDTO
            {
                Id = e.Id,
                Name = e.Translations
                            .FirstOrDefault(t => t.Language == lang)?.Name
                         ?? e.Translations
                            .FirstOrDefault(t => t.Language == "ar")?.Name
            });
        }

        public async Task<IEnumerable<ProblemTypeUserResponseDTO>> GetActiveForUserAsync()
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var list = await _repository.GetActiveForUserAsync();

            return list.Select(e => new ProblemTypeUserResponseDTO
            {
                Id = e.Id,
                Name = e.Translations
                        .FirstOrDefault(t => t.Language == lang)?.Name
                     ?? e.Translations
                        .FirstOrDefault(t => t.Language == "ar")?.Name
            });
        }


        public async Task<ProblemTypeUserResponseDTO?> GetByIdForUserAsync(int id)
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var e = await _repository.GetByIdForUserAsync(id);
            if (e is null) return null;

            return new ProblemTypeUserResponseDTO
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
