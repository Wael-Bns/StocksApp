using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Services;
using StocksApp.OrdersWorker.Stores;
using StocksApp.OrdersWorker.Worker;

namespace StocksApp.OrdersWorker.IoC
{
    public static class WorkerExtensions
    {
        public static IServiceCollection AddWorkerServices(this IServiceCollection services)
        {
            services.AddSingleton<ISellOrdersStore, SellOrdersStore>();
     
            services.AddSingleton<IPendingOrdersInitializer, PendingOrderInitializer>();

            services.AddSingleton<IPriceUpdateOrderProcessor, PriceUpdateOrderProcessor>();
            
            services.AddSingleton<IWorkerSubscriptionsManager, WorkerSubscriptionsManager>();

            return services;
        }
    }
}
