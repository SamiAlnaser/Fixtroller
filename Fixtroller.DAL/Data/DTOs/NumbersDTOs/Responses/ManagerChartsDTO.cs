using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.NumbersDTOs.Responses
{


    public class ManagerChartsDTO
    {
        public List<ChartPointDTO> RequestsByCategory { get; set; } = new();
        public List<StatusDistributionDTO> StatusDistribution { get; set; } = new();
    }
    public class ChartPointDTO
    {
        public string Label { get; set; } = default!;
        public int Count { get; set; }
    }
    public class StatusDistributionDTO
    {
        public string CaseType { get; set; } = default!;
        public int Count { get; set; }
    }
}
