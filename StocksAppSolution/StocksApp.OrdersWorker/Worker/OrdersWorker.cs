using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.OrdersWorker.ServiceContracts;

namespace StocksApp.OrdersWorker.Worker
{
    public class OrdersWorker : BackgroundService
    {
        private readonly ILogger<OrdersWorker> _logger;
        private readonly IFinnhubWebSocketClient _finnhubWebSocketClient;
        private readonly IWorkerSubscriptionsManager _workerSubscriptionsManager;
        private readonly IPriceUpdateOrderProcessor _priceUpdateOrderProcessor;

        public OrdersWorker(
            ILogger<OrdersWorker> logger, 
            IFinnhubWebSocketClient finnhubWebSocketClient, 
            IPriceUpdateOrderProcessor priceUpdateOrderProcessor,
            IWorkerSubscriptionsManager workerSubscriptionsManager)
        {
            _logger = logger;
            _finnhubWebSocketClient = finnhubWebSocketClient;
            _priceUpdateOrderProcessor = priceUpdateOrderProcessor;
            _workerSubscriptionsManager = workerSubscriptionsManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

                _logger.LogInformation("Starting {ServiceName} at: {time}", nameof(OrdersWorker), DateTimeOffset.Now);
                await _finnhubWebSocketClient.ConnectAsync(stoppingToken);
                
                _finnhubWebSocketClient.OnMessageReceived += ProcessPriceUpdates;

                var subscriptionsRefreshTask = _workerSubscriptionsManager.RefreshSubscriptionsPeriodically(TimeSpan.FromMinutes(1),stoppingToken);
            
                var priceUpdateProcessorTask = _priceUpdateOrderProcessor.StartAsync(stoppingToken);
            
                var finnhubTask = _finnhubWebSocketClient.ReceiveAsync(stoppingToken);

                await Task.WhenAll(subscriptionsRefreshTask, finnhubTask, priceUpdateProcessorTask);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred in {ServiceName}: {Message}", nameof(OrdersWorker), ex.Message);
            }
            finally
            {
                _finnhubWebSocketClient.OnMessageReceived -= ProcessPriceUpdates;
                await _finnhubWebSocketClient.DisconnectAsync(CancellationToken.None);
                _logger.LogInformation("{ServiceName} stopped at: {time}",nameof(OrdersWorker), DateTimeOffset.Now);
            }
        }
        private async Task ProcessPriceUpdates(IReadOnlyCollection<PriceUpdateMessage> priceUpdates)
        {
            if (priceUpdates != null)
            {
                foreach (var priceUpdate in priceUpdates)
                {
                    await _priceUpdateOrderProcessor.EnqueueMessageAsync(priceUpdate);
                }
            }
        }
    }
}
