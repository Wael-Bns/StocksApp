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
        private readonly IPriceFeedSubscriptionRegistry _workerSubscriptionsManager;
        public PendingSellOrdersBootstrapper(IPendingOrdersStore sellOrdersStore,
            IServiceScopeFactory serviceScopeFactory,
            IPriceFeedSubscriptionRegistry workerSubscriptionsManager)
        {
            _sellOrdersStore = sellOrdersStore;
            _serviceScopeFactory = serviceScopeFactory;
            _workerSubscriptionsManager = workerSubscriptionsManager;
        }
        public async Task RestoreAsync(CancellationToken cancellationToken)
        {
            var spec = new PendingSellOrdersSpecification();
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IGenericRepository<SellOrder>>();
            var pendingSellOrders = await service.ListAsync(spec);
            foreach(var order in pendingSellOrders)
            {
                _sellOrdersStore.AddSellOrder(order.ToSellOrderCreatedCommand());
                await _workerSubscriptionsManager.EnsureSubscribedAsync(order.StockSymbol!, cancellationToken);
            }
        }
    }
}
