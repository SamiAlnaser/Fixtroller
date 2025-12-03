using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Responses
{
    public sealed class TechnicianTaskCardDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string Priority { get; set; } = "";
        public string CaseType { get; set; } = "";
        public string? ProblemTypeName { get; set; }
        public DateTime CreatedAt { get; set; }    
    }

    public sealed class TechnicianBoardColumnDTO
    {
        public string Title { get; set; } = "";   
        public int Count { get; set; }             
        public IReadOnlyList<TechnicianTaskCardDTO> Items { get; set; }
            = Array.Empty<TechnicianTaskCardDTO>();
    }

    public sealed class TechnicianBoardDTO
    {
        public TechnicianBoardColumnDTO New { get; set; } = new();
        public TechnicianBoardColumnDTO InProgress { get; set; } = new();
        public TechnicianBoardColumnDTO Completed { get; set; } = new();
    }
}
