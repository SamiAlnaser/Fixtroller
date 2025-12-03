using Fixtroller.DAL.Data.DTOs.ProblemTypeDTOs.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.DTOs.ProblemTypeDTOs.Requests
{
    public class ProblemTypeRequestDTO
    {
        [Required]
        [MinLength(1)]
        public List<ProblemTypeTranslationsRequestDTO> Translations { get; set; }
    }
}
