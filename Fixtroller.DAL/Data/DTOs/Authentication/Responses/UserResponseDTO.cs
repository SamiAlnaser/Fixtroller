using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.Authentication.Responses
{
    public class UserResponseDTO
    {
            public string Token { get; set; }
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
    }
}
