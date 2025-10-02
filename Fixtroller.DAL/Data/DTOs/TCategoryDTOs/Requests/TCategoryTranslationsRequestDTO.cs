using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Requests
{
    public class TCategoryTranslationsRequestDTO
    {
        public string Name { get; set; }
        public string Language { get; set; } = "ar";
    }
}
