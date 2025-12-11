using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.AIDTOs
{
    public sealed class AiEmployeeChatRequestDTO
    {
        public string Message { get; set; } = string.Empty;
    }

    public sealed class AiEmployeeChatResponseDTO
    {
        public bool IsEnabled { get; set; }
        public string Reply { get; set; } = string.Empty;
    }

    public sealed class AiEmployeeChatSettingsDTO
    {
        public bool IsEnabled { get; set; }
    }
}
