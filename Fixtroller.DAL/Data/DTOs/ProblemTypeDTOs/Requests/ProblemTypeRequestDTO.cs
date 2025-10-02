using Fixtroller.DAL.Data.DTOs.ProblemTypeDTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.DTOs.ProblemTypeDTOs.Requests
{
    public class ProblemTypeRequestDTO
    {
        public List<ProblemTypeTranslationsRequestDTO> Translations { get; set; }
    }
}
