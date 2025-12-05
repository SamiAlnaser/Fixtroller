using Fixtroller.DAL.Data.DTOs.Authentication.Responses;
using Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs;
using Fixtroller.DAL.Data.DTOs.UsersDTOS.Requset;
using Fixtroller.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.UserServices
{
    public interface IUserservice
    {
        Task<List<UserDTO>> GetAllAsync();

        Task<UserDTO> GetByIdAsync(string userId);
        Task<bool> ChangeUserRoleAsync(ChangeRoleRequsetDTO dto);
        Task<bool> VacationUserAsync(string userId, int days);
        Task<bool> UnVacationUserAsync(string userId);
        Task<bool> IsVacationAsync(string userId);



    }
}
