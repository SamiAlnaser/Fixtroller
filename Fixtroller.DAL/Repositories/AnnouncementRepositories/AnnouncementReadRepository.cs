using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities.Announcements;
using Fixtroller.DAL.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.AnnouncementRepositories
{
    public class AnnouncementReadRepository
       : GenericRepository<AnnouncementRead>, IAnnouncementReadRepository
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementReadRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public IQueryable<AnnouncementRead> Query(
            bool asTracking = false,
            Expression<Func<AnnouncementRead, bool>>? predicate = null)
        {
            IQueryable<AnnouncementRead> q = _context.AnnouncementReads;

            if (!asTracking)
                q = q.AsNoTracking();

            if (predicate is not null)
                q = q.Where(predicate);

            return q;
        }
    }
}
