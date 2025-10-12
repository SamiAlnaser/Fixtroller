using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Repositories.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.MaintenanceRequestepository
{
    public interface IMaintenanceRequestRepository : IGenericRepository<MaintenanceRequest>
    {
        IQueryable<MaintenanceRequest> Query(
            bool asTracking = false,
            Func<IQueryable<MaintenanceRequest>, IQueryable<MaintenanceRequest>>? include = null,
            Expression<Func<MaintenanceRequest, bool>>? predicate = null);
    }
}
