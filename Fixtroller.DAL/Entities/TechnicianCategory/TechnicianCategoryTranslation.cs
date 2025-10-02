using Fixtroller.DAL.Entities.TechnicianCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Fixtroller.DAL.Entities.TechnicianCategory
{
    public class TechnicianCategoryTranslation
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Language { get; set; } = "ar";


        public int TechnicianCategoryyId { get; set; }
        public  TechnicianCategory  TechnicianCategory { get; set; }
    }
}
