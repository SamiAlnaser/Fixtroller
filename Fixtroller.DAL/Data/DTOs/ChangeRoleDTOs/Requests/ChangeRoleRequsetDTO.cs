using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs.Requests
{
   public enum RoleType
    {
        Admin=1,
        Employee=2,
        Technician=3,
        MaintenanceManager = 4
    }
    public class ChangeRoleRequestDTO
    {
        [Required]
        public RoleType NewRoleName { get; set; }
        

        [Required]
        public string UserId { get; set; }
    }
}
