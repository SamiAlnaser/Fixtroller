using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.UsersDTOS.Responses
{
    public sealed class AdminTechniciansAvailabilityNumbersDTO
    {
        public int TotalTechnicians { get; set; }
        public int AvailableTechnicians { get; set; }
        public int VacationTechnicians { get; set; }
    }
}
