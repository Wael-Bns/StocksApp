using Microsoft.IdentityModel.Tokens;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.OrdersWorker.ServiceContracts;

namespace StocksApp.OrdersWorker.Services
{
    public class WorkerSubscriptionsManager : IWorkerSubscriptionsManager
    {
        private readonly ILogger<WorkerSubscriptionsManager> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFinnhubWebSocketClient _finnhubWebSocketClient;
        private readonly HashSet<string> _subscribedStockSymbols;
        public WorkerSubscriptionsManager(ILogger<WorkerSubscriptionsManager> logger, IServiceProvider serviceProvider, IFinnhubWebSocketClient finnhubWebSocketClient)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _finnhubWebSocketClient = finnhubWebSocketClient;
            _subscribedStockSymbols = new HashSet<string>();
        }
        public async Task RefreshSubscriptionsPeriodically(TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var ordersRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                var symbols = await ordersRepository.GetPendingSellOrderSymbols();
            
                foreach (var symbol in symbols)
                {
                    if (!string.IsNullOrEmpty(symbol) && _subscribedStockSymbols.Add(symbol))
                    {
                        _logger.LogInformation("Subscribing to new stock symbol: {Symbol}", symbol);
                        await _finnhubWebSocketClient.SubscribeAsync(symbol, cancellationToken);
                    }
                }

                await Task.Delay(timeSpan, cancellationToken);
            }
        }
    }
}
