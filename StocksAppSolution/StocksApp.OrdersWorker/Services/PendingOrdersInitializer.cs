using StocksApp.Domain.Entities;
using StocksApp.Domain.Events;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Stores;

namespace StocksApp.OrdersWorker.Services
{
    public class PendingOrdersInitializer : IPendingOrdersInitializer
    {
        private readonly ISellOrdersStore _sellOrdersStore;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWorkerSubscriptionsManager _workerSubscriptionsManager;
        public PendingOrdersInitializer(ISellOrdersStore sellOrdersStore,
            IServiceScopeFactory serviceScopeFactory,
            IWorkerSubscriptionsManager workerSubscriptionsManager)
        {
            _sellOrdersStore = sellOrdersStore;
            _serviceScopeFactory = serviceScopeFactory;
            _workerSubscriptionsManager = workerSubscriptionsManager;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var spec = new PendingSellOrdersSpecification();
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IGenericRepository<SellOrder>>();
            var pendingSellOrders = await service.ListAsync(spec);
            foreach(var order in pendingSellOrders)
            {
                _sellOrdersStore.AddSellOrder(order.ToSellOrderCreatedCommand());
                await _workerSubscriptionsManager.AddStockSymbol(order.StockSymbol!, cancellationToken);
            }
        }
    }
}
