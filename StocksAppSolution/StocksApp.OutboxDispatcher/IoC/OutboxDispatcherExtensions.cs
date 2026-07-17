using StocksApp.Core.ServiceContracts;
using StocksApp.OutboxDispatcher.EventHandlers;
using StocksApp.OutboxDispatcher.Services;

namespace StocksApp.OutboxDispatcher.IoC
{
    public static class OutboxDispatcherExtensions
    {
        public static IServiceCollection AddOutboxDispatcherServices(this IServiceCollection services)
        {
            services.AddSingleton<IOutboxNotificationsListener, OutboxNotificationsListener>();
            services.AddSingleton<IOutboxEventHandler, SellOrderCreatedCommandOutboxHandler>();

            services.AddScoped<IOutboxProcessor, OutboxProcessor>();

            return services;
        }
    }
}
