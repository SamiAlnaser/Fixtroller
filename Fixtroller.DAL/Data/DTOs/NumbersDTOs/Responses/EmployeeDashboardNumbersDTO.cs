using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.NumbersDTOs.Responses
{
    public class EmployeeDashboardNumbersDTO
    {
        public int Total { get; set; }
        public int Waiting { get; set; }      // Submitted + ManagerReview + ResourcesNeeded
        public int Processing { get; set; }   // Processing
        public int Completed { get; set; }    // Completed
        public int Cancelled { get; set; }    // Cancelled
    }
}
