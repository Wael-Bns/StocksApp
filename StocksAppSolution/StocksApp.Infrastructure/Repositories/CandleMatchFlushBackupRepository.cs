using Microsoft.EntityFrameworkCore;
using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.Infrastructure.Repositories
{
    public class CandleMatchFlushBackupRepository : ICandleMatchFlushBackupRepository
    {
        private readonly ApplicationDbContext _context;
        public CandleMatchFlushBackupRepository(ApplicationDbContext context) => _context = context;

        public async Task AddAsync(CandleMatchFlushBackup backup, CancellationToken ct)
        {
            _context.CandleMatchFlushBackups.Add(backup);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<List<CandleMatchFlushBackup>> GetPendingAsync(int batchSize, CancellationToken ct)
        {
            return await _context.CandleMatchFlushBackups.AsNoTracking()
                .OrderBy(b => b.FailedAt).Take(batchSize).ToListAsync(ct);
        }

        public async Task RemoveAsync(Guid id, CancellationToken ct)
        {
            await _context.CandleMatchFlushBackups.Where(b => b.Id == id).ExecuteDeleteAsync(ct);
        }

        public async Task<int> CountPendingAsync(CancellationToken ct)
        {
            return await _context.CandleMatchFlushBackups.CountAsync(ct);
        }
    }
}
