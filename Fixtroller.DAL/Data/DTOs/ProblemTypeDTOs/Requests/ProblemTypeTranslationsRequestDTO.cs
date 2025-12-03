using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.ProblemTypeDTOs.Requests
{
    public class ProblemTypeTranslationsRequestDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(5)]
        public string Language { get; set; } = "ar";
    }
}
