using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.OrdersWorker.ServiceContracts;

namespace StocksApp.OrdersWorker
{
    public class OrdersWorker : BackgroundService
    {
        private readonly ILogger<OrdersWorker> _logger;
        private readonly IFinnhubWebSocketClient _finnhubWebSocketClient;
        private readonly IPriceUpdateOrderProcessor _priceUpdateOrderProcessor;

        public OrdersWorker(ILogger<OrdersWorker> logger, IFinnhubWebSocketClient finnhubWebSocketClient, IPriceUpdateOrderProcessor priceUpdateOrderProcessor)
        {
            _logger = logger;
            _finnhubWebSocketClient = finnhubWebSocketClient;
            _priceUpdateOrderProcessor = priceUpdateOrderProcessor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting OrdersWorker at: {time}", DateTimeOffset.Now);
            await _finnhubWebSocketClient.ConnectAsync(stoppingToken);

            // TO DO: Subscribe to the stock symbols you want to track. 

            _finnhubWebSocketClient.OnMessageReceived += async (priceUpdates) => await ProcessPriceUpdates(priceUpdates);

            await _priceUpdateOrderProcessor.StartAsync(stoppingToken);
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
