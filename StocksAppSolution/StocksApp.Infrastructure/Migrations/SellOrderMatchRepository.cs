using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.Infrastructure.Repositories
{
    public class SellOrderMatchRepository : ISellOrderMatchRepository
    {
        private readonly ApplicationDbContext _context;
        public SellOrderMatchRepository(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyList<Guid>> GetMatchableOrderIdsAsync(TimeSpan gracePeriod, CancellationToken ct)
        {
            var conn = (NpgsqlConnection)_context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync(ct);

            // the bucket width and the grace period are both injected as parameters
            // rather than hardcoded, so this stays correct if MatchBucketSize is ever retuned
            await using var cmd = new NpgsqlCommand(@"
                SELECT so.""SellOrderID"" FROM ""SellOrder"" so
                WHERE so.""Status"" = 'Pending'
                  AND EXISTS (
                    SELECT 1 FROM candle_matches_5s m
                    WHERE m.symbol = so.""StockSymbol""
                      AND m.bucket_start >= so.""ActivatesAt""
                      AND m.bucket_start + @bucketWidth + @grace <= now()
                      AND m.high >= so.""Price"")
                FOR UPDATE OF so SKIP LOCKED", conn);

            cmd.Transaction = (NpgsqlTransaction?)_context.Database.CurrentTransaction?.GetDbTransaction();
            cmd.Parameters.AddWithValue("bucketWidth", TimeSpan.FromSeconds(5));
            cmd.Parameters.AddWithValue("grace", gracePeriod);

            var ids = new List<Guid>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                ids.Add(reader.GetGuid(0));

            return ids;
        }

        public async Task<IReadOnlyList<SellOrder>> ListTrackedAsync(IReadOnlyList<Guid> orderIds, CancellationToken ct) =>
            await _context.SellOrders
                .Include(o => o.User)
                .Where(o => orderIds.Contains(o.SellOrderID))
                .ToListAsync(ct);
        public async Task<bool> TryCancelAsync(Guid orderId, TimeSpan gracePeriod, CancellationToken ct)
        {
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""SellOrder"" SET ""Status"" = 'Cancelled'
            WHERE ""SellOrderID"" = {orderId} AND ""Status"" = 'Pending'
              AND NOT EXISTS (
                SELECT 1 FROM candle_matches_5s m
                WHERE m.symbol = ""StockSymbol"" AND m.bucket_start >= ""ActivatesAt""
                  AND m.bucket_start + interval '5 seconds' + {gracePeriod} <= now()
                  AND m.high >= ""Price"")", ct);

            return rows > 0;
        }
    }
}