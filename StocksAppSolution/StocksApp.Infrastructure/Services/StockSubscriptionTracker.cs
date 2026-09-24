using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Infrastructure.Services
{
    public sealed class StockSubscriptionTracker : IStockSubscriptionTracker
    {
        private sealed class SymbolState
        {
            public int RefCount;
            public readonly SemaphoreSlim TransitionLock = new(1, 1);
        }

        private readonly ConcurrentDictionary<string, SymbolState> _symbolStates = new();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _connectionSymbols = new();
        private readonly IPriceFeedSubscriber _priceFeedSubscriber;
        private readonly ILogger<StockSubscriptionTracker> _logger;

        public StockSubscriptionTracker(IPriceFeedSubscriber priceFeedSubscriber, ILogger<StockSubscriptionTracker> logger)
        {
            _priceFeedSubscriber = priceFeedSubscriber;
            _logger = logger;
        }

        public async Task AddInterestAsync(string connectionId, string symbol, CancellationToken ct = default)
        {
            symbol = symbol.Trim().ToUpperInvariant();
            var connSymbols = _connectionSymbols.GetOrAdd(connectionId, _ => new ConcurrentDictionary<string, byte>());

            if (!connSymbols.TryAdd(symbol, 0)) return; // this connection already subscribed

            var state = _symbolStates.GetOrAdd(symbol, _ => new SymbolState());
            await state.TransitionLock.WaitAsync(ct);
            try
            {
                state.RefCount++;
                if (state.RefCount == 1)
                {
                    try
                    {
                        await _priceFeedSubscriber.SubscribeAsync(symbol, ct);
                    }
                    catch
                    {
                        state.RefCount--;
                        connSymbols.TryRemove(symbol, out _);
                        throw;
                    }
                }
            }
            finally
            {
                state.TransitionLock.Release();
            }
        }

        public async Task RemoveInterestAsync(string connectionId, string symbol, CancellationToken ct = default)
        {
            symbol = symbol.Trim().ToUpperInvariant();
            if (!_connectionSymbols.TryGetValue(connectionId, out var connSymbols) || !connSymbols.TryRemove(symbol, out _))
                return;

            await DecrementAsync(symbol, ct);
        }

        public async Task<IReadOnlyCollection<string>> RemoveConnectionAsync(string connectionId, CancellationToken ct = default)
        {
            if (!_connectionSymbols.TryRemove(connectionId, out var connSymbols))
                return Array.Empty<string>();

            var symbols = connSymbols.Keys.ToArray();
            foreach (var symbol in symbols)
            {
                await DecrementAsync(symbol, ct);
            }
            return symbols;
        }

        private async Task DecrementAsync(string symbol, CancellationToken ct)
        {
            if (!_symbolStates.TryGetValue(symbol, out var state)) return;

            await state.TransitionLock.WaitAsync(ct);
            try
            {
                if (state.RefCount == 0) return;
                state.RefCount--;
                if (state.RefCount == 0)
                {
                    try
                    {
                        await _priceFeedSubscriber.UnsubscribeAsync(symbol, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to release interest in {Symbol}", symbol);
                    }
                }
            }
            finally
            {
                state.TransitionLock.Release();
            }
        }
    }
}
