using Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.DTOs.TCategoryDTOs.Requests
{
    public class TCategoryRequestDTO
    {
        public List<TCategoryTranslationsRequestDTO> Translations { get; set; }
    }
}
