using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.UsersDTOs.Responses
{
    public  class UserListItemDTO
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
    }
}
