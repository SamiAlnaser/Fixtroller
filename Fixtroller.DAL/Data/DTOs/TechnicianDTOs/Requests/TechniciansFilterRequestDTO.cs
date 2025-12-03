using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests
{
    public class TechniciansFilterRequestDTO
    {
        public int? TechnicianCategoryId { get; set; }   // فلترة بالقسم
        public string? Search { get; set; }
        [Required]
        [StringLength(5)]
        public string Language { get; set; } = "ar";     // لاختيار ترجمة اسم القسم
    }

}
