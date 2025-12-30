using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.NumbersDTOs.Responses
{
    public class ManagerDashboardNumbersDTO
    {
        public int Total { get; set; }
        public int Processing { get; set; }
        public int Completed { get; set; }
        public int Submitted { get; set; }
        public int ResourcesNeeded { get; set; }
    }
}
