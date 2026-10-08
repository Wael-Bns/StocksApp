using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using StocksApp.OutboxDispatcher.Options;
using StocksApp.OutboxDispatcher.Services;

namespace StocksApp.OutboxDispatcher.IoC
{
    public static class OutboxDispatcherExtensions
    {
        public static IServiceCollection AddOutboxDispatcherServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
            
            services.AddSingleton<IOutboxNotificationsListener, OutboxNotificationsListener>();
            services.AddScoped<IOutboxProcessor, OutboxProcessor>();

            return services;
        }
    }
}
