using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.Candles;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.Infrastructure.Services
{
    public sealed class ScopedCandleStore : ICandleStore
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public ScopedCandleStore(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

        public async Task UpsertClosedCandleAsync(OhlcBar bar, CancellationToken ct)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<ICandleRepository>();
            await repo.UpsertAsync(ToEntity(bar), ct);
        }

        private static Candle1m ToEntity(OhlcBar bar) => new()
        {
            Symbol = bar.Symbol,
            BucketStart = bar.BucketStart,
            Open = bar.Open,
            High = bar.High,
            Low = bar.Low,
            Close = bar.Close,
            Volume = bar.Volume,
            TradeCount = bar.TradeCount
        };
    }
}