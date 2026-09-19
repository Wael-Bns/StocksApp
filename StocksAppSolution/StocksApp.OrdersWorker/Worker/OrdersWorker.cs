using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.OrdersWorker.Channels;
using StocksApp.OrdersWorker.Messages;
using StocksApp.OrdersWorker.ServiceContracts;

namespace StocksApp.OrdersWorker.Worker
{
    /// <summary>
    /// Runs the pending-orders pipeline as a background service and owns its lifecycle
    /// </summary>
    public class OrdersWorker : BackgroundService
    {
        private readonly IFinnhubWebSocketClient _finnhubWebSocketClient;
        private readonly IPendingSellOrdersBootstrapper _pendingOrdersInitializer;
        private readonly IWorkerMessageDispatcher _orderMessageProcessor;
        private readonly IWorkerChannel _channel;
        private ILogger<OrdersWorker> _logger;
        public OrdersWorker(IFinnhubWebSocketClient finnhubWebSocketClient,
            IPendingSellOrdersBootstrapper pendingOrdersInitializer,
            IWorkerMessageDispatcher orderMessageProcessor,
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
                
                _finnhubWebSocketClient.OnPriceUpdatesReceived += ProcessPriceUpdates;

                await _finnhubWebSocketClient.ConnectAsync(cancellationToken);

                await _pendingOrdersInitializer.RestoreAsync(cancellationToken);

                var orderMessageProcessorTask = _orderMessageProcessor.RunAsync(cancellationToken);

                var finnhubTask = _finnhubWebSocketClient.ReceiveLoopAsync(cancellationToken);

                await Task.WhenAll(orderMessageProcessorTask, finnhubTask);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred in {ServiceName}: {Message}", nameof(OrdersWorker), ex.Message);
            }
            finally
            {
                _finnhubWebSocketClient.OnPriceUpdatesReceived -= ProcessPriceUpdates;
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
