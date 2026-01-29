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
    public interface IAnnouncementRepository : IGenericRepository<Announcement>
    {
        IQueryable<Announcement> Query(
            bool asTracking = false,
            Func<IQueryable<Announcement>, IQueryable<Announcement>>? include = null,
            Expression<Func<Announcement, bool>>? predicate = null);
    }
}
