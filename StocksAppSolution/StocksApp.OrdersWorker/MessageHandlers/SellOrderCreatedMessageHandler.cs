using StocksApp.OrdersWorker.Messages;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Stores;

namespace StocksApp.OrdersWorker.MessageHandlers
{
    /// <summary>
    /// Reacts to newly created sell orders.
    /// </summary>
    public sealed class SellOrderCreatedMessageHandler : WorkerMessageHandler<SellOrderCreatedWorkerMessage>
    {
        private readonly ILogger<SellOrderCreatedMessageHandler> _logger;
        private readonly IPriceFeedSubscriptionRegistry _workerSubscriptionsManager;
        private readonly IPendingOrdersStore _sellOrdersStore;

        public SellOrderCreatedMessageHandler(ILogger<SellOrderCreatedMessageHandler> logger,
            IPriceFeedSubscriptionRegistry workerSubscriptionsManager,
            IPendingOrdersStore sellOrdersStore)
        {
            _logger = logger;
            _workerSubscriptionsManager = workerSubscriptionsManager;
            _sellOrdersStore = sellOrdersStore;
        }

        protected override async Task HandleAsync(SellOrderCreatedWorkerMessage message, CancellationToken cancellationToken = default)
        {
            await _workerSubscriptionsManager.EnsureSubscribedAsync(message.SellOrder.StockSymbol, cancellationToken);
            _sellOrdersStore.AddSellOrder(message.SellOrder);
        }
    }
}
