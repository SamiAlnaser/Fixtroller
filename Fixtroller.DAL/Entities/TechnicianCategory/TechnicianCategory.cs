using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Entities.TechnicianCategory
{
    public class TechnicianCategory : BaseModel
    {
        public List<TechnicianCategoryTranslation> Translations { get; set; } = new List<TechnicianCategoryTranslation>();

    }
}
