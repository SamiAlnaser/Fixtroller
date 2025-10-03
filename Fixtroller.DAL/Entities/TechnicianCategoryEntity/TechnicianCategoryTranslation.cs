using Fixtroller.DAL.Entities.TechnicianCategoryEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Fixtroller.DAL.Entities.TechnicianCategoryEntity
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
