using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.Data.DTOs.ProblemTypeDTOs.Responses;
using Fixtroller.DAL.DTOs.ProblemTypeDTOs.Requests;
using Fixtroller.DAL.DTOs.ProblemTypeDTOs.Responses;
using Fixtroller.DAL.Entities.ProblemTypeEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.ProblemTypesServices
{
    public interface IProblemTypesService : IGenericService<ProblemTypeRequestDTO, ProblemTypeResponseDTO, ProblemType>
    {
        Task<IEnumerable<ProblemTypeUserResponseDTO>> GetAllForUserAsync();
        Task<IEnumerable<ProblemTypeUserResponseDTO>> GetActiveForUserAsync();
        Task<ProblemTypeUserResponseDTO?> GetByIdForUserAsync(int id);
    }
}
