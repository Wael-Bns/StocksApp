using StocksApp.OrdersWorker.Messages;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Stores;

namespace StocksApp.OrdersWorker.MessageHandlers
{
    public sealed class SellOrderCreatedMessageHandler : WorkerMessageHandler<SellOrderCreatedWorkerMessage>
    {
        private readonly ILogger<SellOrderCreatedMessageHandler> _logger;
        private readonly IWorkerSubscriptionsManager _workerSubscriptionsManager;
        private readonly ISellOrdersStore _sellOrdersStore;

        public SellOrderCreatedMessageHandler(ILogger<SellOrderCreatedMessageHandler> logger,
            IWorkerSubscriptionsManager workerSubscriptionsManager,
            ISellOrdersStore sellOrdersStore)
        {
            _logger = logger;
            _workerSubscriptionsManager = workerSubscriptionsManager;
            _sellOrdersStore = sellOrdersStore;
        }

        protected override async Task HandleAsync(SellOrderCreatedWorkerMessage message, CancellationToken cancellationToken = default)
        {
            _sellOrdersStore.AddSellOrder(message.SellOrder);
            await _workerSubscriptionsManager.AddStockSymbol(message.SellOrder.StockSymbol, cancellationToken);
        }
    }
}
