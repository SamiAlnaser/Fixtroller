using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.UsersDTOs.Requset
{
    public sealed class VacationUserRequestDTO
    {
        [Range(1, 365, ErrorMessage = "Days must be between 1 and 365.")]
        public int Days { get; set; }
    }
}
