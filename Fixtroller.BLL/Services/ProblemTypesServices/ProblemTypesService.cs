using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.DTOs.ProblemTypeDTOs.Requests;
using Fixtroller.DAL.DTOs.ProblemTypeDTOs.Responses;
using Fixtroller.DAL.Entities.ProblemType;
using Fixtroller.DAL.Repositories.ProblemTypeRepositories;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.ProblemTypesServices
{
    public class ProblemTypesService : GenericService<ProblemTypeRequestDTO, ProblemTypeResponseDTO, ProblemType> , IProblemTypesService
    {
        public ProblemTypesService(IProblemTypeRepository repository) : base(repository) { }

    }
}
