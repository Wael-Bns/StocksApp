using StocksApp.Domain.Events;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Stores;

namespace StocksApp.OrdersWorker.Services
{
    public class PendingOrderInitializer : IPendingOrdersInitializer
    {
        private readonly ISellOrdersStore _sellOrdersStore;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public PendingOrderInitializer(ISellOrdersStore sellOrdersStore, IServiceScopeFactory serviceScopeFactory)
        {
            _sellOrdersStore = sellOrdersStore;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var spec = new PendingSellOrdersSpecification();
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var pendingSellOrders = await service.GetSellOrdersBySpecificationAsNoTracking(spec);
            foreach(var order in pendingSellOrders)
            {
                _sellOrdersStore.AddSellOrder(order.ToSellOrderCreatedCommand());
            }
        }
    }
}
