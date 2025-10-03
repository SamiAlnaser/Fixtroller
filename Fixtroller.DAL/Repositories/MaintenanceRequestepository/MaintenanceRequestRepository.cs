using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.GenericRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.MaintenanceRequestepository
{
    public class MaintenanceRequestRepository : GenericRepository<MaintenanceRequest>, IMaintenanceRequestRepository
    {
        public MaintenanceRequestRepository(ApplicationDbContext context) : base(context)
        {
            
        }

    }
}
