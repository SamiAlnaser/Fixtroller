using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.NumbersRepository
{
    public class MetricsRepository : IMetricsRepository
    {
        private readonly ApplicationDbContext _context;

        public MetricsRepository(ApplicationDbContext context) => _context = context;

        public async Task<int> CountActiveRequestsForTechnicianAsync(
            string technicianUserId,
            CancellationToken ct = default)
        {
            var activeCases = new[]
            {
            CaseType.Submitted, CaseType.ManagerReview, CaseType.Processing,
            CaseType.ResourcesNeeded, CaseType.Reopened, CaseType.Modified
        };

            return await _context.MaintenanceRequestTechnicians
                .AsNoTracking()
                .Where(rt => rt.TechnicianUserId == technicianUserId && rt.UnassignedAtUtc == null)
                .Where(rt => activeCases.Contains(rt.Request.CaseType))
                .Select(rt => rt.RequestId)
                .Distinct()
                .CountAsync(ct);
        }

        public async Task<double> AverageCompletionHoursForTechnicianAsync(
            string technicianUserId,
            CancellationToken ct = default)
        {
            // نجمع مدة العمل لكل طلب، ثم نأخذ متوسط المدّة على الطلبات المكتملة فقط
            var perRequestSeconds = await _context.WorkTimeEntries
                .AsNoTracking()
                .Where(w => w.TechnicianUserId == technicianUserId && w.StoppedAt != null)
                .GroupBy(w => w.RequestId)
                .Select(g => new
                {
                    RequestId = g.Key,
                    TotalSeconds = g.Sum(w => EF.Functions.DateDiffSecond(w.StartedAt, w.StoppedAt!.Value))
                })
                .Join(_context.MaintenanceRequests.Where(r => r.CaseType == CaseType.Completed),
                      w => w.RequestId, r => r.Id,
                      (w, r) => w.TotalSeconds)
                .ToListAsync(ct);

            if (perRequestSeconds.Count == 0) return 0.0;
            var avgSeconds = perRequestSeconds.Average();
            return avgSeconds / 3600.0; 
        }

        public async Task<int> CountRequestsByCasesForTechnicianAsync(
    string technicianUserId,
    IEnumerable<CaseType> cases,
    CancellationToken ct = default)
        {
            var set = (cases ?? Enumerable.Empty<CaseType>()).ToHashSet();
            if (set.Count == 0) return 0;

            return await _context.MaintenanceRequestTechnicians
                .AsNoTracking()
                .Where(rt => rt.TechnicianUserId == technicianUserId && rt.UnassignedAtUtc == null)
                .Where(rt => set.Contains(rt.Request.CaseType))
                .Select(rt => rt.RequestId)
                .Distinct()
                .CountAsync(ct);
        }



        public async Task<double> SumWorkSecondsForTechnicianAsync(
            string technicianUserId,
            DateTimeOffset? fromUtc = null,
            DateTimeOffset? toUtc = null,
            CancellationToken ct = default)
        {
            var q = _context.WorkTimeEntries.AsNoTracking()
                .Where(w => w.TechnicianUserId == technicianUserId);

            if (fromUtc.HasValue) q = q.Where(w => w.StartedAt >= fromUtc.Value);
            if (toUtc.HasValue) q = q.Where(w => (w.StoppedAt ?? DateTimeOffset.UtcNow) <= toUtc.Value);

            var list = await q.Select(w =>
                EF.Functions.DateDiffSecond(w.StartedAt, (w.StoppedAt ?? DateTimeOffset.UtcNow))).ToListAsync(ct);

            return list.Sum();
        }
        public async Task<int> CountAllRequestsAsync(CancellationToken ct = default)
       => await _context.MaintenanceRequests.AsNoTracking().CountAsync(ct);

        // عدد الطلبات بحسب مجموعة حالات
        public async Task<int> CountRequestsByCasesAsync(IEnumerable<CaseType> cases, CancellationToken ct = default)
        {
            var set = (cases ?? Enumerable.Empty<CaseType>()).ToHashSet();
            if (set.Count == 0) return 0;

            return await _context.MaintenanceRequests
                .AsNoTracking()
                .Where(r => set.Contains(r.CaseType))
                .CountAsync(ct);
        }
        public async Task<int> CountNewRequestsForTechnicianAsync(string techId, CancellationToken ct = default)
        {
            var q = _context.MaintenanceRequestTechnicians
                .AsNoTracking()
                .Where(rt => rt.TechnicianUserId == techId && rt.UnassignedAtUtc == null)
                .Select(rt => rt.Request);

            var reopened = q.Where(r => r.CaseType == CaseType.Reopened);

            var notStarted = q.Where(r => r.CaseType == CaseType.Processing)
                              .Where(r => !_context.WorkTimeEntries
                                  .Any(w => w.RequestId == r.Id && w.TechnicianUserId == techId));

            return await reopened
                .Union(notStarted)         
                .Select(r => r.Id)
                .Distinct()
                .CountAsync(ct);
        }

        public async Task<int> CountAllRequestsForOwnerAsync(string ownerUserId, CancellationToken ct = default)
        => await _context.MaintenanceRequests
                     .AsNoTracking()
                     .Where(r => r.CreatedByUserId == ownerUserId)
                     .CountAsync(ct);

        public async Task<int> CountRequestsByCasesForOwnerAsync(
            string ownerUserId,
            IEnumerable<CaseType> cases,
            CancellationToken ct = default)
        {
            var set = (cases ?? Enumerable.Empty<CaseType>()).ToHashSet();
            if (set.Count == 0) return 0;

            return await _context.MaintenanceRequests
                .AsNoTracking()
                .Where(r => r.CreatedByUserId == ownerUserId && set.Contains(r.CaseType))
                .CountAsync(ct);
        }

        public async Task<List<(int CategoryId, string Name, int Count)>> GetRequestsByTechnicianCategoryAsync(
       string language = "ar",
       DateTimeOffset? fromUtc = null,
       DateTimeOffset? toUtc = null,
       CancellationToken ct = default)
        {
            // روابط التعيين النشطة فقط
            var activeLinks = _context.MaintenanceRequestTechnicians
                .AsNoTracking()
                .Where(rt => rt.UnassignedAtUtc == null);

            // انضمام للمستخدم للحصول على فئة الفني
            var baseQ = activeLinks
                .Join(_context.Users.AsNoTracking().Select(u => new { u.Id, u.TechnicianCategoryId }),
                      rt => rt.TechnicianUserId, u => u.Id,
                      (rt, u) => new { rt.RequestId, u.TechnicianCategoryId })
                .Where(x => x.TechnicianCategoryId != null)
                .Select(x => new { x.RequestId, CategoryId = x.TechnicianCategoryId!.Value })
                .Distinct(); // (طلب، فئة) مرة واحدة

            // فلترة زمنية (اختياري) عبر تاريخ إنشاء الطلب
            if (fromUtc.HasValue || toUtc.HasValue)
            {
                var reqTimes = _context.MaintenanceRequests
                    .AsNoTracking()
                    .Select(r => new { r.Id, r.CreatedAt });

                baseQ = baseQ
                    .Join(reqTimes, b => b.RequestId, r => r.Id, (b, r) => new { b.RequestId, b.CategoryId, r.CreatedAt })
                    .Where(x => !fromUtc.HasValue || x.CreatedAt >= fromUtc.Value.UtcDateTime)
                    .Where(x => !toUtc.HasValue || x.CreatedAt <= toUtc.Value.UtcDateTime)
                    .Select(x => new { x.RequestId, x.CategoryId })
                    .Distinct();
            }

            // جلب اسم الفئة مترجم
            var q = baseQ
                .Join(
                    _context.Tcategories
                        .Include(c => c.Translations)
                        .AsNoTracking(),
                    b => b.CategoryId,
                    c => c.Id,
                    (b, c) => new
                    {
                        CategoryId = c.Id,
                        Name =
                            c.Translations
                             .Where(t => t.Language == language)
                             .Select(t => t.Name)
                             .FirstOrDefault()
                            ?? c.Translations.Select(t => t.Name).FirstOrDefault()
                            ?? "غير مُصنّف"
                    })
                .GroupBy(z => new { z.CategoryId, z.Name })
                .Select(g => new { g.Key.CategoryId, g.Key.Name, Count = g.Count() })
                .OrderByDescending(x => x.Count);

            var rows = await q.ToListAsync(ct);
            return rows.Select(x => (x.CategoryId, x.Name, x.Count)).ToList();
        }

        public async Task<List<(CaseType Case, int Count)>> GetRequestsStatusDistributionAsync(
            DateTimeOffset? fromUtc = null,
            DateTimeOffset? toUtc = null,
            CancellationToken ct = default)
        {
            var rq = _context.MaintenanceRequests.AsNoTracking();

            if (fromUtc.HasValue) rq = rq.Where(r => r.CreatedAt >= fromUtc.Value.UtcDateTime);
            if (toUtc.HasValue) rq = rq.Where(r => r.CreatedAt <= toUtc.Value.UtcDateTime);

            var rows = await rq
                .GroupBy(r => r.CaseType)
                .Select(g => new { Case = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(ct);

            return rows.Select(x => (x.Case, x.Count)).ToList();
        }

        public async Task<List<(string TechnicianUserId, int AssignedCount, int CompletedCount)>>
       GetRequestStatsPerTechnicianAsync(IEnumerable<string> technicianIds, CancellationToken ct = default)
        {
            var ids = technicianIds.Distinct().ToList();
            if (ids.Count == 0) return new();

            var rows = await _context.MaintenanceRequests
                .AsNoTracking()
                .SelectMany(r => r.Technicians.Select(t => new
                {
                    t.TechnicianUserId,
                    IsActiveAssign = (t.UnassignedAtUtc == null),
                    r.CaseType
                }))
                .Where(x => ids.Contains(x.TechnicianUserId))
                .GroupBy(x => x.TechnicianUserId)
                .Select(g => new
                {
                    TechnicianUserId = g.Key,
                    AssignedCount = g.Count(x => x.IsActiveAssign),
                    CompletedCount = g.Count(x => x.CaseType == CaseType.Completed)
                })
                .ToListAsync(ct);

            return rows.Select(r => (r.TechnicianUserId, r.AssignedCount, r.CompletedCount)).ToList();
        }

        public async Task<List<(string TechnicianUserId, int AvgCompletionMinutes)>>
            GetAvgCompletionMinutesPerTechnicianAsync(IEnumerable<string> technicianIds, CancellationToken ct = default)
        {
            var ids = technicianIds.Distinct().ToList();
            if (ids.Count == 0) return new();

            var perRequest = _context.WorkTimeEntries
                .AsNoTracking()
                .Where(w => w.StoppedAt != null && ids.Contains(w.TechnicianUserId))
                .GroupBy(w => new { w.TechnicianUserId, w.RequestId })
                .Select(g => new
                {
                    g.Key.TechnicianUserId,
                    g.Key.RequestId,
                    TotalSeconds = g.Sum(w => EF.Functions.DateDiffSecond(w.StartedAt, w.StoppedAt!.Value))
                });

            var rows = await perRequest
                .Join(_context.MaintenanceRequests.AsNoTracking(),
                      x => x.RequestId,
                      r => r.Id,
                      (x, r) => new { x.TechnicianUserId, x.TotalSeconds, r.CaseType })
                .Where(z => z.CaseType == CaseType.Completed)
                .GroupBy(z => z.TechnicianUserId)
                .Select(g => new
                {
                    TechnicianUserId = g.Key,
                    AvgMinutes = (int)Math.Round(g.Average(z => z.TotalSeconds) / 60.0)
                })
                .ToListAsync(ct);

            return rows.Select(r => (r.TechnicianUserId, r.AvgMinutes)).ToList();
        }


    }
}
