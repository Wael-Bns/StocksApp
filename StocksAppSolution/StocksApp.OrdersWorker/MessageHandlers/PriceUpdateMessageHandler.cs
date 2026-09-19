using StocksApp.Core.ServiceContracts;
using StocksApp.OrdersWorker.Messages;
using StocksApp.OrdersWorker.Stores;

namespace StocksApp.OrdersWorker.MessageHandlers
{
    /// <summary>
    /// Reacts to market price changes.
    /// </summary>
    public sealed class PriceUpdateMessageHandler : WorkerMessageHandler<PriceUpdateWorkerMessage>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPendingOrdersStore _sellOrdersStore;

        public PriceUpdateMessageHandler(
            IServiceScopeFactory scopeFactory, IPendingOrdersStore sellOrdersStore)
        {
            _scopeFactory = scopeFactory;
            _sellOrdersStore = sellOrdersStore;
        }

        protected override async Task HandleAsync(PriceUpdateWorkerMessage message, CancellationToken cancellationToken = default)
        {
            var eligibleOrders = _sellOrdersStore.TakeTriggeredOrders(message.StockSymbol, message.Price);

            if (eligibleOrders.Count == 0) return;

            using var scope = _scopeFactory.CreateScope();
            var ordersExecutionService = scope.ServiceProvider.GetRequiredService<IOrdersExecutionService>();

            await ordersExecutionService.ExecuteSellOrdersAsync(eligibleOrders, cancellationToken);
        }
    }
}
