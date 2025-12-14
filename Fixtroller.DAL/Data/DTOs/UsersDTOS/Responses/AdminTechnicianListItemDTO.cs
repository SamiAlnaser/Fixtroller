using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.UsersDTOS.Responses
{
    public sealed class AdminTechnicianListItemDTO
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? TechnicianCategoryName { get; set; }
        public bool IsVacation { get; set; }
    }
}
