using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.PagedResultDTOs.Responses
{
    public class PagedResultDTO<T>
    {
        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public List<T> Data { get; set; } = new List<T>();

    }

}
