using Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Requests
{
    public class TCategoryRequestDTO
    {
        [Required]
        [MinLength(1)]
        public List<TCategoryTranslationsRequestDTO> Translations { get; set; }
    }
}
