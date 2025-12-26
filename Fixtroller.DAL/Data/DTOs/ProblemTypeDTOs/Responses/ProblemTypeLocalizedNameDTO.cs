using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.ProblemTypeDTOs.Responses
{
    public class ProblemTypeLocalizedNameDTO
    {
        public string Language { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class ProblemTypeDetailsResponseDTO
    {
        public int Id { get; set; }

        // كل الأسماء بكل اللغات
        public List<ProblemTypeLocalizedNameDTO> Names { get; set; } = new();
    }
}
