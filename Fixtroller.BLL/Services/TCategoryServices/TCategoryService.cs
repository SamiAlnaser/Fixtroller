using Fixtroller.BLL.Services.GenericService;
using Fixtroller.DAL.DTOs.TCategoryDTOs.Requests;
using Fixtroller.DAL.DTOs.TCategoryDTOs.Responses;
using Fixtroller.DAL.Entities.TCategory;
using Fixtroller.DAL.Repositories.TCategoryRepositories;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.TCategoryServices
{
    public class TCategoryService : GenericService<TCategoryRequestDTO, TCategoryResponseDTO, TechnicianCategory> , ITCategoryService
    {
        public TCategoryService(ITCategoryRepository repository) : base(repository) { }



    }
}
