using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.NotificationDTOs.Responses
{
    public sealed class NotificationLoadMoreResponseDTO<TItem>
    {
        public List<TItem> Items { get; set; } = new();
        public bool HasMore { get; set; }
        public int? NextLastId { get; set; }
    }
}
