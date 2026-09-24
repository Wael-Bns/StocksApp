using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Events;
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
        private readonly IPriceFeedSubscriber _priceFeedSubscriber;
        private readonly IPendingSellOrdersBootstrapper _pendingSellOrdersBootstrapper;
        private readonly IWorkerMessageDispatcher _workerMessageDispatcher;
        private readonly IWorkerChannel _channel;
        private readonly ILogger<OrdersWorker> _logger;
        public OrdersWorker(IPriceFeedSubscriber priceFeedSubscriber,
            IPendingSellOrdersBootstrapper pendingSellOrdersBootstrapper,
            IWorkerMessageDispatcher workerMessageDispatcher,
            IWorkerChannel channel,
            ILogger<OrdersWorker> logger)
        {
            _priceFeedSubscriber = priceFeedSubscriber;
            _pendingSellOrdersBootstrapper = pendingSellOrdersBootstrapper;
            _workerMessageDispatcher = workerMessageDispatcher;
            _channel = channel;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting {ServiceName} at: {time}", nameof(OrdersWorker), DateTimeOffset.Now);
            _priceFeedSubscriber.OnPriceTick += ProcessPriceTick;

            try
            {
                await _pendingSellOrdersBootstrapper.RestoreAsync(cancellationToken);
                await _workerMessageDispatcher.RunAsync(cancellationToken);

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
                _priceFeedSubscriber.OnPriceTick -= ProcessPriceTick;
                _logger.LogInformation("{ServiceName} stopped at: {time}", nameof(OrdersWorker), DateTimeOffset.Now);
            }
        }

        private async Task ProcessPriceTick(IPriceTickPublished priceTick, CancellationToken cancellationToken)
        {
            if (priceTick is null) return;

            try
            {
                var priceUpdateWorkerMessage = priceTick.ToPriceUpdateWorkerMessage();
                await _channel.EnqueueAsync(priceUpdateWorkerMessage, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue price update for {Symbol}; tick dropped", priceTick.StockSymbol);
            }
        }
    }
}
