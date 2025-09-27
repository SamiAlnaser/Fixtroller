using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities.TCategory;
using Fixtroller.DAL.Repositories.GenericRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.TCategoryRepositories
{
    public class TCategorRepository : GenericRepository<TechnicianCategory>, ITCategoryRepository
    {


        public TCategorRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
