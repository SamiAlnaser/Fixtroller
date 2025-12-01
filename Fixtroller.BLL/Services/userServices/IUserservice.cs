using Fixtroller.DAL.Data.DTOs.Authentication.Responses;
using Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs;
using Fixtroller.DAL.Data.DTOs.UsersDTOS.Requset;
using Fixtroller.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.userServices
{
    public interface IUserservice
    {
        Task<List<UserDto>> GetAllAsync();

        Task<UserDto> GetByIdAsync(string userId);
        Task<bool> ChangeUserRoleAsync(ChangeRoleRequsetDTO dto);
        Task<bool> BlockUserAsync(string userId, int days);
        Task<bool> UnBlockUserAsync(string userId);
        Task<bool> IsBlockedAsync(string userId);



    }
}
