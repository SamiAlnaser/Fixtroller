using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Responses
{
    public class TCategoryLocalizedNameDTO
    {
        public string Language { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class TCategoryDetailsResponseDTO
    {
        public int Id { get; set; }
        public List<TCategoryLocalizedNameDTO> Names { get; set; } = new();
    }
}
