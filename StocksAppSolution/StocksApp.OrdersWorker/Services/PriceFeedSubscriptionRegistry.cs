using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.OrdersWorker.ServiceContracts;

namespace StocksApp.OrdersWorker.Services
{
    public class PriceFeedSubscriptionRegistry : IPriceFeedSubscriptionRegistry
    {
        private readonly IFinnhubWebSocketClient _finnhubWebSocketClient;
        private readonly HashSet<string> _subscribedStockSymbols;
        public PriceFeedSubscriptionRegistry(IFinnhubWebSocketClient finnhubWebSocketClient)
        {
            _finnhubWebSocketClient = finnhubWebSocketClient;
            _subscribedStockSymbols = new HashSet<string>();
        }

        public async Task EnsureSubscribedAsync(string stockSymbol, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(stockSymbol) || _subscribedStockSymbols.Contains(stockSymbol))
                return;

            await _finnhubWebSocketClient.SubscribeAsync(stockSymbol, cancellationToken);
            _subscribedStockSymbols.Add(stockSymbol);
        }
        public async Task UnsubscribeAsync(string stockSymbol, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(stockSymbol) && _subscribedStockSymbols.Remove(stockSymbol))
            {
                await _finnhubWebSocketClient.UnsubscribeAsync(stockSymbol, cancellationToken);
            }
        }
    }
}
