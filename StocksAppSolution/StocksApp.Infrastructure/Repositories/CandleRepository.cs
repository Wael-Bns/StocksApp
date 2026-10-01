using Microsoft.EntityFrameworkCore;
using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.Infrastructure.Repositories
{
    public class CandleRepository : ICandleRepository
    {
        private readonly ApplicationDbContext _context;
        public CandleRepository(ApplicationDbContext context) => _context = context;
        public async Task UpsertAsync(Candle1m candle, CancellationToken ct)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO candles_1m (symbol, bucket_start, open, high, low, close, volume, trade_count)
                VALUES ({candle.Symbol}, {candle.BucketStart}, {candle.Open}, {candle.High},
                        {candle.Low}, {candle.Close}, {candle.Volume}, {candle.TradeCount})
                ON CONFLICT (symbol, bucket_start) DO UPDATE SET
                    open = EXCLUDED.open, high = EXCLUDED.high, low = EXCLUDED.low,
                    close = EXCLUDED.close, volume = EXCLUDED.volume, trade_count = EXCLUDED.trade_count",
                ct);
        }
    }
}