using Azure;
using Azure.Core;
using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.Data.DTOs.ProblemTypeDTOs.Responses;
using Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Responses;
using Fixtroller.DAL.DTOs.TCategoryDTOs.Requests;
using Fixtroller.DAL.DTOs.TCategoryDTOs.Responses;
using Fixtroller.DAL.Entities.TechnicianCategoryEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.TCategoryServices
{
    public interface ITCategoryService : IGenericService<TCategoryRequestDTO, TCategoryResponseDTO, TechnicianCategory>
    {
        Task<IEnumerable<TCategoryUserResponseDTO>> GetAllForUserAsync();
        Task<IEnumerable<TCategoryUserResponseDTO>> GetActiveForUserAsync();
        Task<TCategoryUserResponseDTO?> GetByIdForUserAsync(int id);
    }
}
