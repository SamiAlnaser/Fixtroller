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
    public class AnnouncementRepository
          : GenericRepository<Announcement>, IAnnouncementRepository
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public IQueryable<Announcement> Query(
            bool asTracking = false,
            Func<IQueryable<Announcement>, IQueryable<Announcement>>? include = null,
            Expression<Func<Announcement, bool>>? predicate = null)
        {
            IQueryable<Announcement> q = _context.Announcements;

            if (!asTracking)
                q = q.AsNoTracking();

            if (include is not null)
                q = include(q);

            if (predicate is not null)
                q = q.Where(predicate);

            return q;
        }
    }
}
