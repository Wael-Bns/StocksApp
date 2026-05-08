using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using StocksApp.Core.WebSocketClientAstractions;

namespace StocksApp.WebApi.Hubs
{
    /// <summary>
    /// Responsible for sending real-time stock price updates to clients subscribed to specific stock symbols.
    /// </summary>
    public class StocksHub : Hub
    {
        private static readonly ConcurrentDictionary<string, int> SubscriptionCounts = new();
        private readonly ILogger<StocksHub> _logger;
        private readonly IFinnhubWebSocketClient _finnhubWebSocketClient;

        public StocksHub(ILogger<StocksHub> logger, IFinnhubWebSocketClient finnhubWebSocketClient)
        {
            _logger = logger;
            _finnhubWebSocketClient = finnhubWebSocketClient;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation(
                $"Client connected to {nameof(StocksHub)}: {Context.ConnectionId}");

            var httpContext = Context.GetHttpContext();
            string? symbol = httpContext?.Request.Query["symbol"].ToString();

            if (!string.IsNullOrEmpty(symbol))
            {
                int newCount = SubscriptionCounts.AddOrUpdate(symbol, 1, (_, count) => count + 1);

                if (newCount == 1)
                {
                    _logger.LogInformation("Subscribing to symbol: {symbol}", symbol);
                    await _finnhubWebSocketClient.SubscribeAsync(symbol);
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, symbol);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            string? symbol = httpContext?.Request.Query["symbol"].ToString();

            _logger.LogInformation(
                $"Client disconnected from {nameof(StocksHub)}: {Context.ConnectionId}");

            if (!string.IsNullOrEmpty(symbol))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol);

                if (SubscriptionCounts.TryGetValue(symbol, out int count))
                {
                    int newCount = SubscriptionCounts.AddOrUpdate(symbol, 0, (_, count) => count - 1);

                    if (newCount <= 0)
                    {
                        SubscriptionCounts.TryRemove(symbol, out _);
                        _logger.LogInformation("Unsubscribing from symbol: {symbol} as there are no more subscribers", symbol);
                        await _finnhubWebSocketClient.UnsubscribeAsync(symbol);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
