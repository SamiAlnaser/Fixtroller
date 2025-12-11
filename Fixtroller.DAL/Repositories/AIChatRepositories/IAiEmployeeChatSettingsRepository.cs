using Fixtroller.DAL.Entities.AICHAT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Repositories.AIChatRepositories
{
    public interface IAiEmployeeChatSettingsRepository
    {
        Task<AiEmployeeChatSettingsEntity?> GetAsync(CancellationToken ct = default);
        Task AddAsync(AiEmployeeChatSettingsEntity entity, CancellationToken ct = default);
    }
}
