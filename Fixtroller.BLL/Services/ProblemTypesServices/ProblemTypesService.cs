using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.Data.DTOs.ProblemTypeDTOs.Responses;
using Fixtroller.DAL.DTOs.ProblemTypeDTOs.Requests;
using Fixtroller.DAL.DTOs.ProblemTypeDTOs.Responses;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.ProblemTypeEntity;
using Fixtroller.DAL.Repositories.ProblemTypeRepositories;
using Fixtroller.DAL.UnitOfWork;
using Mapster;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.ProblemTypesServices
{
    public class ProblemTypesService
    : GenericService<ProblemTypeRequestDTO, ProblemTypeResponseDTO, ProblemType>,
      IProblemTypesService
    {
        private readonly IProblemTypeRepository _repository;

        public ProblemTypesService(IProblemTypeRepository repository , IUnitOfWork uow)
            : base(repository , uow)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProblemTypeUserResponseDTO>> GetAllForUserAsync(
            string language,
            bool? isActive,
            CancellationToken ct = default)
        {
            var list = await _repository.GetAllForUserAsync(
                isActive: isActive,
                asTracking: false,
                ct: ct);

            return list.Select(e => new ProblemTypeUserResponseDTO
            {
                Id = e.Id,
                Name = e.Translations
                          .FirstOrDefault(t => t.Language == language)?.Name
                       ?? e.Translations
                          .FirstOrDefault(t => t.Language == "ar")?.Name,
                Status = GetStatusName(e.Status, language)
            });
        }


        public async Task<IEnumerable<ProblemTypeUserResponseDTO>> GetActiveForUserAsync(
            string language,
            CancellationToken ct = default)
        {
            var list = await _repository.GetActiveForUserAsync(asTracking: false, ct);

            return list.Select(e => new ProblemTypeUserResponseDTO
            {
                Id = e.Id,
                Name = e.Translations
                         .FirstOrDefault(t => t.Language == language)?.Name
                      ?? e.Translations
                         .FirstOrDefault(t => t.Language == "ar")?.Name
            });
        }

        public async Task<ProblemTypeDetailsResponseDTO?> GetByIdForUserAsync(
            int id,
            CancellationToken ct = default)
        {
            // استخدم نفس الريبو اللي بتستخدمه للـ GetById
            var e = await _repository.GetByIdForUserAsync(id, ct);
            if (e is null)
                return null;

            var dto = new ProblemTypeDetailsResponseDTO
            {
                Id = e.Id,
                Names = e.Translations?
                    .Select(t => new ProblemTypeLocalizedNameDTO
                    {
                        Language = t.Language,
                        Name = t.Name
                    })
                    .ToList() ?? new List<ProblemTypeLocalizedNameDTO>()
            };

            return dto;
        }


        private static string GetStatusName(Status status, string language)
        {
            var isAr = string.Equals(language, "ar", StringComparison.OrdinalIgnoreCase);

            if (isAr)
            {
                return status switch
                {
                    Status.Active => "فعال",
                    Status.In_active => "غير فعال",
                    _ => status.ToString()
                };
            }

            return status switch
            {
                Status.Active => "Active",
                Status.In_active => "Inactive",
                _ => status.ToString()
            };
        }
    }
}
