using StocksApp.OrdersWorker.Channels;
using StocksApp.OrdersWorker.MessageHandlers;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Services;
using StocksApp.OrdersWorker.Stores;

namespace StocksApp.OrdersWorker.IoC
{
    public static class WorkerExtensions
    {
        public static IServiceCollection AddWorkerServices(this IServiceCollection services)
        {
            services.AddSingleton<IWorkerChannel, WorkerChannel>();

            services.AddSingleton<ISellOrdersStore, SellOrdersStore>();
     
            services.AddSingleton<IPendingOrdersInitializer, PendingOrdersInitializer>();
            
            services.AddSingleton<IOrderMessageProcessor, OrderMessageProcessor>();

            services.AddSingleton<IWorkerMessageHandler, PriceUpdateMessageHandler>();
            
            services.AddSingleton<IWorkerMessageHandler, SellOrderCreatedMessageHandler>();
            
            services.AddSingleton<IWorkerSubscriptionsManager, WorkerSubscriptionsManager>();

            return services;
        }
    }
}
