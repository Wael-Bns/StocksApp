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
        private readonly ILogger<PriceUpdateMessageHandler> _logger;

        public PriceUpdateMessageHandler(
            IServiceScopeFactory scopeFactory, 
            IPendingOrdersStore sellOrdersStore, 
            ILogger<PriceUpdateMessageHandler> logger)
        {
            _scopeFactory = scopeFactory;
            _sellOrdersStore = sellOrdersStore;
            _logger = logger;
        }

        protected override async Task HandleAsync(PriceUpdateWorkerMessage message, CancellationToken cancellationToken = default)
        {
            var eligibleOrders = _sellOrdersStore.TakeTriggeredOrders(message.StockSymbol, message.Price);

            if (eligibleOrders.Count == 0) return;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var ordersExecutionService = scope.ServiceProvider.GetRequiredService<IOrdersExecutionService>();

                await ordersExecutionService.ExecuteSellOrdersAsync(eligibleOrders, cancellationToken);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error while handling price update message for stock {StockSymbol} with price {Price}: {ExceptionMessage}", message.StockSymbol, message.Price, ex.Message);
                foreach(var order in eligibleOrders)
                {
                    _sellOrdersStore.AddSellOrder(order);
                }
            }
        }
    }
}
