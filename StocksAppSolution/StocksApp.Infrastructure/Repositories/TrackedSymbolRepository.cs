using Microsoft.EntityFrameworkCore;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.Infrastructure.Repositories
{
    public class TrackedSymbolRepository : ITrackedSymbolRepository
    {
        private readonly ApplicationDbContext _context;

        public TrackedSymbolRepository(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyList<string>> GetActiveSymbolsAsync(CancellationToken ct)
        {
            return await _context.TrackedSymbols
                .AsNoTracking()
                .Where(t => t.IsActive)
                .Select(t => t.Symbol)
                .ToListAsync(ct);
        }
    }
}