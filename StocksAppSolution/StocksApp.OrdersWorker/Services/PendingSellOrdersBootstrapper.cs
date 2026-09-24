using StocksApp.Domain.Entities;
using StocksApp.Domain.Events;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Stores;

namespace StocksApp.OrdersWorker.Services
{
    public class PendingSellOrdersBootstrapper : IPendingSellOrdersBootstrapper
    {
        private readonly IPendingOrdersStore _sellOrdersStore;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IPriceFeedSubscriptionRegistry _priceFeedSubscriptionRegistry;
        private readonly ILogger<PendingSellOrdersBootstrapper> _logger;
        public PendingSellOrdersBootstrapper(IPendingOrdersStore sellOrdersStore,
            IServiceScopeFactory serviceScopeFactory,
            IPriceFeedSubscriptionRegistry priceFeedSubscriptionRegistry,
            ILogger<PendingSellOrdersBootstrapper> logger)
        {
            _sellOrdersStore = sellOrdersStore;
            _serviceScopeFactory = serviceScopeFactory;
            _priceFeedSubscriptionRegistry = priceFeedSubscriptionRegistry;
            _logger = logger;
        }
        public async Task RestoreAsync(CancellationToken cancellationToken)
        {
            var spec = new PendingSellOrdersSpecification();
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IGenericRepository<SellOrder>>();
            var pendingSellOrders = await service.ListAsync(spec);
            foreach(var order in pendingSellOrders)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await _priceFeedSubscriptionRegistry.EnsureSubscribedAsync(order.StockSymbol!, cancellationToken);
                    _sellOrdersStore.AddSellOrder(order.ToSellOrderCreatedCommand());
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex,
                        "Failed to restore pending sell order {OrderId} for symbol {Symbol}; skipping",
                        order.SellOrderID, order.StockSymbol);
                }
            }
        }
    }
}
