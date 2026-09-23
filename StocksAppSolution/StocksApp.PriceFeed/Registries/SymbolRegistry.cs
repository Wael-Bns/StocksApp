using System.Collections.Concurrent;
using StocksApp.Core.WebSocketClientAbstractions;

namespace StocksApp.PriceFeed.Registries
{
    public sealed class SymbolRegistry : ISymbolRegistry
    {
        private sealed class SymbolState
        {
            public int RefCount;
            public readonly SemaphoreSlim TransitionLock = new(1, 1);
        }

        private readonly ConcurrentDictionary<string, SymbolState> _symbols = new();
        private readonly IFinnhubWebSocketClient _client;
        private readonly ILogger<SymbolRegistry> _logger;

        public SymbolRegistry(IFinnhubWebSocketClient client, ILogger<SymbolRegistry> logger)
        {
            _client = client;
            _logger = logger;
        }

        public IReadOnlyCollection<string> ActiveSymbols =>
            _symbols.Where(kv => kv.Value.RefCount > 0).Select(kv => kv.Key).ToArray();

        public async Task AddInterestAsync(string symbol, CancellationToken ct = default)
        {
            symbol = Normalize(symbol);
            var state = _symbols.GetOrAdd(symbol, _ => new SymbolState());

            await state.TransitionLock.WaitAsync(ct);
            try
            {
                state.RefCount++;
                if (state.RefCount == 1)
                {
                    await _client.SubscribeAsync(symbol, ct);
                    _logger.LogInformation("Subscribed to {Symbol}", symbol);
                }
            }
            catch
            {
                state.RefCount--;
                throw;
            }
            finally
            {
                state.TransitionLock.Release();
            }
        }

        public async Task RemoveInterestAsync(string symbol, CancellationToken ct = default)
        {
            symbol = Normalize(symbol);
            if (!_symbols.TryGetValue(symbol, out var state))
            {
                _logger.LogWarning("ReleaseSymbol for {Symbol} with no tracked demand — ignoring", symbol);
                return;
            }

            await state.TransitionLock.WaitAsync(ct);
            try
            {
                if (state.RefCount == 0) return;
                state.RefCount--;
                if (state.RefCount == 0)
                {
                    await _client.UnsubscribeAsync(symbol, ct);
                    _logger.LogInformation("Unsubscribed from {Symbol}", symbol);
                }
            }
            finally
            {
                state.TransitionLock.Release();
            }
        }

        private static string Normalize(string symbol) => symbol.Trim().ToUpperInvariant();
    }
}
