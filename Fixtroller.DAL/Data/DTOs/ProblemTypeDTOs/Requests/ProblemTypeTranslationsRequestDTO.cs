using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.ProblemTypeDTOs.Requests
{
    public class ProblemTypeTranslationsRequestDTO
    {
        public string Name { get; set; }
        public string Language { get; set; } = "ar";
    }
}
