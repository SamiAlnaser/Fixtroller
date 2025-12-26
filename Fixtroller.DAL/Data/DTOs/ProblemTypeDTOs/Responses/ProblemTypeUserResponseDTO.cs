using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.ProblemTypeDTOs.Responses
{
    public class ProblemTypeUserResponseDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Status { get; set; }
    }
}
