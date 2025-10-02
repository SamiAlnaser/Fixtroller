using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.ProblemType
{
    public class ProblemTypeTranslation
    {

        public int Id { get; set; }

        public string Name { get; set; }

        public string Language { get; set; } = "ar";


        public int ProblemTypeId { get; set; }
        public ProblemType ProblemType { get; set; }
    }
}
