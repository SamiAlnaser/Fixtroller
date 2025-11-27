using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs
{
   public enum roletype
    {
        Admin=1,
        Employee=2,
        Technician=3,
        manager= 4
    }
    public class ChangeRoleRequsetDTO
    {
        public roletype rolename { get; set; }
        public string userId { get; set; }
    }
}
