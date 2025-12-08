using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.MaintenanceRequestRepositories
{
    public class MaintenanceNoteRepository : GenericRepository<MaintenanceNote>, IMaintenanceNoteRepository
    {
        public MaintenanceNoteRepository(ApplicationDbContext context) : base(context) { }
    }
}
