using Fixtroller.DAL.Data.DTOs.Authentication.Responses;
using Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs;
using Fixtroller.DAL.Data.DTOs.UsersDTOS.Requset;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.UserRepository;
using Mapster;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.UserServices
{
    public class UserService : IUserservice
    {
        private readonly IUserRepository _userRepository;

        private readonly UserManager<ApplicationUser> _userManager;
        public UserService(IUserRepository userRepository, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;

        }

        public async Task<List<UserDTO>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = new List<UserDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userDtos.Add(new UserDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                   
                    RoleName = roles.FirstOrDefault()
                });
            }

            return userDtos;
        }





        public async Task<UserDTO> GetByIdAsync(string userId)
        {
            var users = await _userRepository.GetByIdAsync(userId);
            return users.Adapt<UserDTO>();
        }

        public async Task<bool> ChangeUserRoleAsync(ChangeRoleRequsetDTO dto)
        {
            var rolename = Enum.GetName(typeof(roletype), dto.rolename);

            return await _userRepository.ChangeUserRoleAsync(dto.userId, rolename);
        }

        public async Task<bool> VacationUserAsync(string userId, int days)
        {
            return await _userRepository.VacationUserAsync(userId, days);

        }

        public async Task<bool> UnVacationUserAsync(string userId)
        {
            return await _userRepository.UnVacationUserAsync(userId); 
        }

        public async Task<bool> IsVacationAsync(string userId)
        {
              return await _userRepository.IsVacationAsync(userId);
        }
    }

}
