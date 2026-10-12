using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Infrastructure.Caching
{
    public sealed class NullLatestPriceCacheWriter : ILatestPriceCacheWriter
    {
        public Task WriteAsync(PriceUpdateMessage tick, CancellationToken ct) => Task.CompletedTask;
        public Task DeleteAsync(string symbol, CancellationToken ct) => Task.CompletedTask;
    }
}
