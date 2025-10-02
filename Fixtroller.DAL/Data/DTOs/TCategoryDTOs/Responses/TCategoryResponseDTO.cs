using Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.DTOs.TCategoryDTOs.Responses
{
    public class TCategoryResponseDTO
    {
        public int Id { get; set; }
        public List<TCategoryTranslationsResponseDTO> TCategoryTranslations { get; set; }
    }
}
