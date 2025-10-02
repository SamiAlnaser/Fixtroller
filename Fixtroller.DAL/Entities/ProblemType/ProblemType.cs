using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.ProblemType
{
    public class ProblemType : BaseModel
    {
        public List<ProblemTypeTranslation> Translations {  get; set; } = new List<ProblemTypeTranslation>();
    }
}
