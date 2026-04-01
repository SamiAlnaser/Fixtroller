using Fixtroller.DAL.Entities.Announcements;
using Fixtroller.DAL.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.AnnouncementRepositories
{
    public interface IAnnouncementReadRepository : IGenericRepository<AnnouncementRead>
    {
        IQueryable<AnnouncementRead> Query(
            bool asTracking = false,
            Expression<Func<AnnouncementRead, bool>>? predicate = null);
    }
}
