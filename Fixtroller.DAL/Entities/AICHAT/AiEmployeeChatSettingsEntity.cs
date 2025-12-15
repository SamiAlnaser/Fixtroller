using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.AIChat
{
    public class AiEmployeeChatSettingsEntity
    {
        public int Id { get; set; }
        public bool IsEmployeeEnabled { get; set; }   
        public bool IsTechnicianEnabled { get; set; }
    }
}
