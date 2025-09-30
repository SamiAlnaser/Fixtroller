using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities.ProblemType;
using Fixtroller.DAL.Repositories.GenericRepository;
using Fixtroller.DAL.Repositories.ProblemTypeRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.ProblemTypeRepositories
{
    public class ProblemTypeRepository : GenericRepository<ProblemType>, IProblemTypeRepository
    {


        public ProblemTypeRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
