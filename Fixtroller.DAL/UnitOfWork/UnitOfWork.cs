using Fixtroller.DAL.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.UnitOfWork
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbcontext;
        private IDbContextTransaction? _tx;

        public UnitOfWork(ApplicationDbContext dbcontext) => _dbcontext = dbcontext;

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_tx != null) return;
            _tx = await _dbcontext.Database.BeginTransactionAsync(ct);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _dbcontext.SaveChangesAsync(ct);

        public async Task CommitAsync(CancellationToken ct = default)
        {
            if (_tx is null) return;
            await _tx.CommitAsync(ct);
            await _tx.DisposeAsync();
            _tx = null;
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_tx is null) return;
            await _tx.RollbackAsync(ct);
            await _tx.DisposeAsync();
            _tx = null;
        }

        public async Task<int> SaveAndCommitAsync(CancellationToken ct = default)
        {
            var affected = await _dbcontext.SaveChangesAsync(ct);
            if (_tx != null)
            {
                await _tx.CommitAsync(ct);
                await _tx.DisposeAsync();
                _tx = null;
            }
            return affected;
        }

        public async ValueTask DisposeAsync()
        {
            if (_tx != null)
            {
                await _tx.DisposeAsync();
                _tx = null;
            }
            await _dbcontext.DisposeAsync();
        }
    }
}
