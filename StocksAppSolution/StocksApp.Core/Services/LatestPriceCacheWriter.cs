// StocksApp.Core/Services/LatestPriceTracker.cs
using Microsoft.Extensions.Logging;
using StocksApp.Core.DTO.CandleDTO;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public sealed class LatestPriceCacheWriter : ILatestPriceCacheWriter
    {
        private readonly ICandleCache _cache;
        private readonly ILogger<LatestPriceCacheWriter> _logger;

        public LatestPriceCacheWriter(ICandleCache cache, ILogger<LatestPriceCacheWriter> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task WriteAsync(PriceUpdateMessage tick, CancellationToken ct)
        {
            try
            {
                await _cache.SetLatestPriceAsync(
                    new LatestPriceSnapshot(tick.StockSymbol, (decimal)tick.Price, (long)tick.Volume, tick.Timestamp),
                    ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist latest price for {Symbol} to cache.", tick.StockSymbol);
            }
        }

        public async Task DeleteAsync(string symbol, CancellationToken ct)
        {
            try
            {
                await _cache.DeleteLatestPriceAsync(symbol, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete latest-price cache entry for {Symbol}.", symbol);
            }
        }
    }
}