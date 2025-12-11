using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities.AICHAT;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.AIChatRepositories
{
    public sealed class AiEmployeeChatSettingsRepository : IAiEmployeeChatSettingsRepository
    {
        private readonly ApplicationDbContext _db;

        public AiEmployeeChatSettingsRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<AiEmployeeChatSettingsEntity?> GetAsync(CancellationToken ct = default)
        {
            // مش لازم AsNoTracking هون، لأنه أريح للتعديل لاحقًا
            return _db.AiEmployeeChatSettings.FirstOrDefaultAsync(ct);
        }

        public Task AddAsync(AiEmployeeChatSettingsEntity entity, CancellationToken ct = default)
        {
            return _db.AiEmployeeChatSettings.AddAsync(entity, ct).AsTask();
        }
    }
}
