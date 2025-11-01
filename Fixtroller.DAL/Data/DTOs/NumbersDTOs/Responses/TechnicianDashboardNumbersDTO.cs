using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.NumbersDTOs.Responses
{
    public class TechnicianDashboardNumbersDTO
    {
        public int NewRequests { get; set; }        // Submitted + Reopened
        public int Processing { get; set; }         // Processing
        public int Completed { get; set; }          // Completed
    }
}
