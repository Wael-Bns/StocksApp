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
        private readonly IWorkerMessageDispatcher _workerMessageDispatcher;
        private readonly IWorkerChannel _channel;
        private ILogger<OrdersWorker> _logger;
        public OrdersWorker(IFinnhubWebSocketClient finnhubWebSocketClient,
            IPendingSellOrdersBootstrapper pendingOrdersInitializer,
            IWorkerMessageDispatcher workerMessageDispatcher,
            IWorkerChannel channel,
            ILogger<OrdersWorker> logger)
        {
            _finnhubWebSocketClient = finnhubWebSocketClient;
            _pendingOrdersInitializer = pendingOrdersInitializer;
            _workerMessageDispatcher = workerMessageDispatcher;
            _channel = channel; 
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting {ServiceName} at: {time}", nameof(OrdersWorker), DateTimeOffset.Now);

            using var loopsCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _finnhubWebSocketClient.OnPriceUpdatesReceived += ProcessPriceUpdates;
            try
            {
                await _finnhubWebSocketClient.ConnectAsync(cancellationToken);

                await _pendingOrdersInitializer.RestoreAsync(cancellationToken);

                var consumerTask = _workerMessageDispatcher.RunAsync(cancellationToken);

                var finnhubTask = _finnhubWebSocketClient.ReceiveLoopAsync(cancellationToken);

                var firstStoppedTask = await Task.WhenAny(consumerTask, finnhubTask);
                await firstStoppedTask;

                if (!cancellationToken.IsCancellationRequested)
                {
                    throw new InvalidOperationException("A worker loop ended unexpectedly. This should not happen unless the service is stopping.");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("{ServiceName} is stopping due to cancellation at: {time}", nameof(OrdersWorker), DateTimeOffset.Now);
            }
            finally
            {
                loopsCts.Cancel();
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
