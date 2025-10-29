using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.UnitOfWork
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task BeginTransactionAsync(CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
        Task<int> SaveAndCommitAsync(CancellationToken ct = default);
    }
}
