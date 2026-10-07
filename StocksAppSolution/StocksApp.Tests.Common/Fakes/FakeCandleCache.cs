using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using StocksApp.Core.DTO.CandleDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Tests.Common.Fakes
{
    public class FakeCandleCache : ICandleCache
    {
        private readonly ConcurrentDictionary<string, LatestPriceSnapshot> _latestPrices =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, ActiveBarSnapshot> _activeBars =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Test-only: resets the singleton between tests.</summary>
        public void Clear()
        {
            _latestPrices.Clear();
            _activeBars.Clear();
        }
        public Task SetLatestPriceAsync(LatestPriceSnapshot snapshot, CancellationToken ct)
        {
            _latestPrices[snapshot.Symbol] = snapshot;
            return Task.CompletedTask;
        }

        public Task<LatestPriceSnapshot?> GetLatestPriceAsync(string symbol, CancellationToken ct)
        {
            _latestPrices.TryGetValue(symbol, out var snapshot);
            return Task.FromResult(snapshot);
        }

        public Task DeleteLatestPriceAsync(string symbol, CancellationToken ct)
        {
            _latestPrices.TryRemove(symbol, out _);
            return Task.CompletedTask;
        }

        public Task SetActiveBarAsync(ActiveBarSnapshot bar, CancellationToken ct)
        {
            _activeBars[bar.Symbol] = bar;
            return Task.CompletedTask;
        }

        public Task<ActiveBarSnapshot?> GetActiveBarAsync(string symbol, CancellationToken ct)
        {
            _activeBars.TryGetValue(symbol, out var bar);
            return Task.FromResult(bar);
        }

        public Task DeleteActiveBarAsync(string symbol, CancellationToken ct)
        {
            _activeBars.TryRemove(symbol, out _);
            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<ActiveBarSnapshot> GetAllActiveBarsAsync(
            [EnumeratorCancellation] CancellationToken ct)
        {
            await Task.CompletedTask;

            foreach (var bar in _activeBars.Values.ToArray())
            {
                ct.ThrowIfCancellationRequested();
                yield return bar;
            }
        }
    }
}