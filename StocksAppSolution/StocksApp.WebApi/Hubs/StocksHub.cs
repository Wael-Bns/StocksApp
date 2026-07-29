using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using StocksApp.Core.WebSocketClientAbstractions;

namespace StocksApp.WebApi.Hubs
{
    public class StocksHub : Hub
    {
        private static readonly ConcurrentDictionary<string, int> SubscriptionCounts = new();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> ConnectionSymbols = new();

        private readonly ILogger<StocksHub> _logger;
        private readonly IFinnhubWebSocketClient _finnhubWebSocketClient;

        public StocksHub(ILogger<StocksHub> logger, IFinnhubWebSocketClient finnhubWebSocketClient)
        {
            _logger = logger;
            _finnhubWebSocketClient = finnhubWebSocketClient;
        }

        public async Task SubscribeToSymbol(string symbol)
        {
            symbol = symbol.ToUpperInvariant();
            var symbols = ConnectionSymbols.GetOrAdd(Context.ConnectionId, _ => new ConcurrentDictionary<string, byte>());

            if (symbols.TryAdd(symbol, 0))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, symbol);
                int newCount = SubscriptionCounts.AddOrUpdate(symbol, 1, (_, count) => count + 1);
                if (newCount == 1)
                {
                    await _finnhubWebSocketClient.SubscribeAsync(symbol);
                }
            }
        }

        public async Task UnsubscribeFromSymbol(string symbol)
        {
            symbol = symbol.ToUpperInvariant();
            if (ConnectionSymbols.TryGetValue(Context.ConnectionId, out var symbols) && symbols.TryRemove(symbol, out _))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol);
                int newCount = SubscriptionCounts.AddOrUpdate(symbol, 0, (_, count) => Math.Max(0, count - 1));
                if (newCount <= 0)
                {
                    SubscriptionCounts.TryRemove(symbol, out _);
                    await _finnhubWebSocketClient.UnsubscribeAsync(symbol);
                }
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected to {nameof(StocksHub)}: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected from {nameof(StocksHub)}: {Context.ConnectionId}");

            if (ConnectionSymbols.TryRemove(Context.ConnectionId, out var symbols))
            {
                foreach (var symbol in symbols.Keys)
                {
                    int newCount = SubscriptionCounts.AddOrUpdate(symbol, 0, (_, count) => Math.Max(0, count - 1));
                    if (newCount <= 0)
                    {
                        SubscriptionCounts.TryRemove(symbol, out _);
                        await _finnhubWebSocketClient.UnsubscribeAsync(symbol);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}