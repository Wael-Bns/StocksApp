using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.OrdersWorker.Channels;
using StocksApp.OrdersWorker.Messages;
using StocksApp.OrdersWorker.ServiceContracts;

namespace StocksApp.OrdersWorker.Worker
{
    public class OrdersWorker : BackgroundService
    {
        private readonly IFinnhubWebSocketClient _finnhubWebSocketClient;
        private readonly IPendingOrdersInitializer _pendingOrdersInitializer;
        private readonly IOrderMessageProcessor _orderMessageProcessor;
        private readonly IWorkerChannel _channel;
        private ILogger<OrdersWorker> _logger;
        public OrdersWorker(IFinnhubWebSocketClient finnhubWebSocketClient,
            IPendingOrdersInitializer pendingOrdersInitializer,
            IOrderMessageProcessor orderMessageProcessor,
            IWorkerChannel channel,
            ILogger<OrdersWorker> logger)
        {
            _finnhubWebSocketClient = finnhubWebSocketClient;
            _pendingOrdersInitializer = pendingOrdersInitializer;
            _orderMessageProcessor = orderMessageProcessor;
            _channel = channel; 
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {

                _logger.LogInformation("Starting {ServiceName} at: {time}", nameof(OrdersWorker), DateTimeOffset.Now);
                
                _finnhubWebSocketClient.OnMessageReceived += ProcessPriceUpdates;

                await _finnhubWebSocketClient.ConnectAsync(cancellationToken);

                await _pendingOrdersInitializer.StartAsync(cancellationToken);

                var orderMessageProcessorTask = _orderMessageProcessor.StartAsync(cancellationToken);

                var finnhubTask = _finnhubWebSocketClient.ReceiveAsync(cancellationToken);

                await Task.WhenAll(orderMessageProcessorTask, finnhubTask);
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
                    var priceUpdateWorkerMessage = priceUpdate.ToPriceUpdateWorkerMessage();
                    await _channel.EnqueueAsync(priceUpdateWorkerMessage);
                }
            }
        }
    }
}
