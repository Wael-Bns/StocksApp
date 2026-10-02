using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StocksApp.Core.Candles;
using StocksApp.Core.Diagnostics;
using StocksApp.Core.DTO.CandleDTO;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public sealed class OhlcBarAggregator : IOhlcBarAggregator
    {
        private sealed class SymbolState
        {
            public OhlcBar? Bar;
            public DateTimeOffset? LastFlushedBucket;
            public readonly SemaphoreSlim Lock = new(1, 1);
        }

        private readonly ConcurrentDictionary<string, SymbolState> _states = new(StringComparer.Ordinal);
        private readonly ICandleCache _cache;
        private readonly ICandleStore _store;
        private readonly ICandleMetrics _metrics;
        private readonly ILatestPriceCacheWriter _latestPriceCacheWriter;
        private readonly TimeSpan _bucketSize;
        private readonly ILogger<OhlcBarAggregator> _logger;

        public OhlcBarAggregator(
            ICandleCache cache,
            ICandleStore store,
            ICandleMetrics metrics,
            ILatestPriceCacheWriter latestPriceCacheWriter,
            IOptions<CandleCacheOptions> options,
            ILogger<OhlcBarAggregator> logger)
        {
            _cache = cache;
            _store = store;
            _metrics = metrics;
            _latestPriceCacheWriter = latestPriceCacheWriter;
            _bucketSize = options.Value.BucketSize;
            _logger = logger;
        }

        public async Task ApplyTickAsync(PriceUpdateMessage tick, CancellationToken ct)
        {
            var bucket = BucketStart(tick.Timestamp);
            var state = _states.GetOrAdd(tick.StockSymbol, _ => new SymbolState());

            await state.Lock.WaitAsync(ct);
            try
            {
                if (state.Bar is { } current)
                {
                    if (bucket == current.BucketStart)
                    {
                        state.Bar = current.Apply(tick);
                        await PersistActiveAsync(state.Bar, ct);
                        return;
                    }

                    if (bucket < current.BucketStart)
                    {
                        _metrics.TickLateForClosedBucket();
                        return;
                    }

                    await FlushAsync(tick.StockSymbol, current, ct);
                    state.LastFlushedBucket = current.BucketStart;
                }
                else if (state.LastFlushedBucket is { } last && bucket <= last)
                {
                    // arrived after this bucket already flushed — never silently restart it
                    _metrics.TickLateForClosedBucket();
                    return;
                }

                state.Bar = OhlcBar.StartNew(tick, bucket);
                await PersistActiveAsync(state.Bar, ct);
            }
            finally
            {
                state.Lock.Release();
            }
        }

        public async Task FlushIdleBarsAsync(DateTimeOffset now, CancellationToken ct)
        {
            foreach (var (symbol, state) in _states)
            {
                await state.Lock.WaitAsync(ct);
                try
                {
                    if (state.Bar is { } bar && now >= bar.BucketStart + _bucketSize)
                    {
                        await FlushAsync(symbol, bar, ct);
                        state.LastFlushedBucket = bar.BucketStart;
                        state.Bar = null;
                    }
                }
                finally
                {
                    state.Lock.Release();
                }
            }
        }

        public async Task HydrateFromCacheAsync(CancellationToken ct)
        {
            _states.Clear();
            await foreach (var snapshot in _cache.GetAllActiveBarsAsync(ct))
            {
                var bar = OhlcBar.FromSnapshot(snapshot.Symbol, snapshot.BucketStart,
                    snapshot.Open, snapshot.High, snapshot.Low, snapshot.Close,
                    snapshot.Volume, snapshot.TradeCount);
                _states[snapshot.Symbol] = new SymbolState { Bar = bar };
            }
        }

        public async Task FlushAndRemoveSymbolAsync(string symbol, CancellationToken ct)
        {
            if (_states.TryGetValue(symbol, out var state))
            {
                await state.Lock.WaitAsync(ct);
                try
                {
                    if (state.Bar is { } bar)
                    {
                        await FlushAsync(symbol, bar, ct);   // also deletes the active-bar key
                        state.Bar = null;
                    }
                }
                finally
                {
                    state.Lock.Release();
                }

                _states.TryRemove(symbol, out _);
            }

            try
            {
                await _latestPriceCacheWriter.DeleteAsync(symbol, ct);
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

        public void Reset() => _states.Clear();

        private DateTimeOffset BucketStart(long unixMillis)
        {
            var dt = DateTimeOffset.FromUnixTimeMilliseconds(unixMillis);
            var ticksPerBucket = _bucketSize.Ticks;
            var flooredTicks = (dt.UtcTicks / ticksPerBucket) * ticksPerBucket;
            return new DateTimeOffset(flooredTicks, TimeSpan.Zero);
        }

        private async Task PersistActiveAsync(OhlcBar bar, CancellationToken ct)
        {
            try
            {
                await _cache.SetActiveBarAsync(ToSnapshot(bar), ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist active bar for {Symbol} to cache.", bar.Symbol);
            }
        }

        private async Task FlushAsync(string symbol, OhlcBar bar, CancellationToken ct)
        {
            try
            {
                await _store.UpsertClosedCandleAsync(bar, ct);
                _metrics.CandleFlushed();
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // no retry — log, count, accept the gap
                _metrics.CandleFlushFailed();
                _logger.LogWarning(ex, "Failed to flush candle for {Symbol} bucket {Bucket}; candle lost (no retry).",
                    symbol, bar.BucketStart);
            }

            try
            {
                await _cache.DeleteActiveBarAsync(symbol, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete active-bar cache entry for {Symbol} after flush.", symbol);
            }
        }

        private static ActiveBarSnapshot ToSnapshot(OhlcBar bar) => new(
            bar.Symbol, bar.BucketStart, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume, bar.TradeCount);
    }
}