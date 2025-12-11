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

        public List<AiChatHistoryItemDTO>? History { get; set; }
    }

    public sealed class AiEmployeeChatResponseDTO
    {
        public bool IsEnabled { get; set; }
        public string Reply { get; set; } = string.Empty;
    }

    public sealed class AiEmployeeChatSettingsDTO
    {
        public bool IsEmployeeEnabled { get; set; }
        public bool IsTechnicianEnabled { get; set; }
    }

    public sealed class AiChatHistoryItemDTO
    {
        public string Role { get; set; } = "user";

        public string Content { get; set; } = string.Empty;
    }
}
