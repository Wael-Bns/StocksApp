using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;

namespace StocksApp.Core
{
    public static class CoreExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddScoped<IFinnHubService, FinnhubService>();

            services.AddScoped<IStockService, StockService>();
            
            services.AddScoped<ITradeExecutionService, TradeExecutionService>();
            
            services.AddScoped<IFinnhubWebSocketService, FinnhubWebSocketService>();

            return services;
        }
    }
}
