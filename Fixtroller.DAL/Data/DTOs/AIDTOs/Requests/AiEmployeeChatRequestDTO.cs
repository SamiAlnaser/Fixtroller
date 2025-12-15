using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.AIDTOs.Requests
{
    public sealed class AiEmployeeChatRequestDTO
    {
        public string Message { get; set; } = string.Empty;

        public List<AiChatHistoryItemDTO>? History { get; set; }
    }


    public sealed class AiChatHistoryItemDTO
    {
        public string Role { get; set; } = "user";

        public string Content { get; set; } = string.Empty;
    }

}
