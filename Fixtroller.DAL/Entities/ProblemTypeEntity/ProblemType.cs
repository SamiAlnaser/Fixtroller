using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.ProblemTypeEntity
{
    public class ProblemType : BaseModel
    {
        public List<MaintenanceRequest> Requests { get; set; } = new List<MaintenanceRequest>();
        public List<ProblemTypeTranslation> Translations {  get; set; } = new List<ProblemTypeTranslation>();
    }
}
