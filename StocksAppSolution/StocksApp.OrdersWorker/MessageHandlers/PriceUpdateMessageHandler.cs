using StocksApp.Core.ServiceContracts;
using StocksApp.OrdersWorker.Messages;
using StocksApp.OrdersWorker.Stores;

namespace StocksApp.OrdersWorker.MessageHandlers
{
    public sealed class PriceUpdateMessageHandler : IWorkerMessageHandler<PriceUpdateWorkerMessage>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISellOrdersStore _sellOrdersStore;

        public PriceUpdateMessageHandler(
            IServiceScopeFactory scopeFactory, ISellOrdersStore sellOrdersStore)
        {
            _scopeFactory = scopeFactory;
            _sellOrdersStore = sellOrdersStore;
        }

        public async Task HandleAsync(PriceUpdateWorkerMessage message, CancellationToken cancellationToken = default)
        {
            var eligibleOrders = _sellOrdersStore.DequeueEligibleOrders(message.StockSymbol, message.Price);

            if (eligibleOrders.Count == 0) return;

            using var scope = _scopeFactory.CreateScope();
            var ordersExecutionService = scope.ServiceProvider.GetRequiredService<IOrdersExecutionService>();

            await ordersExecutionService.ExecuteSellOrdersAsync(eligibleOrders, cancellationToken);
        }
    }
}
