using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.OrdersWorker.ServiceContracts;

namespace StocksApp.OrdersWorker.Services
{
    public class WorkerSubscriptionsManager : IWorkerSubscriptionsManager
    {
        private readonly IFinnhubWebSocketClient _finnhubWebSocketClient;
        private readonly HashSet<string> _subscribedStockSymbols;
        public WorkerSubscriptionsManager(IFinnhubWebSocketClient finnhubWebSocketClient)
        {
            _finnhubWebSocketClient = finnhubWebSocketClient;
            _subscribedStockSymbols = new HashSet<string>();
        }

        public async Task AddStockSymbol(string stockSymbol, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(stockSymbol) && _subscribedStockSymbols.Add(stockSymbol))
            {
                await _finnhubWebSocketClient.SubscribeAsync(stockSymbol, cancellationToken);
            }
        }
    }
}
