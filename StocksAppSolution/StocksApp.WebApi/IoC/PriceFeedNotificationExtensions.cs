using StocksApp.Core.ServiceContracts;
using StocksApp.Infrastructure.Services;
using StocksApp.WebApi.HostedServices;
using StocksApp.WebApi.Notifications;

namespace StocksApp.WebApi.IoC
{
    public static class PriceFeedNotificationExtensions
    {
        public static IServiceCollection AddPriceFeedNotifications(this IServiceCollection services)
        {
            services.AddSingleton<IPriceTickNotifier, SignalRPriceTickNotifier>();
            services.AddSingleton<IStockSubscriptionTracker, StockSubscriptionTracker>();
            services.AddHostedService<StockPricesHostedService>();

            return services;
        }
    }
}
