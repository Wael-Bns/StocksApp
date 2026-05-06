using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;

namespace StocksApp.Core
{
    public static class CoreExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IFinnHubService, FinnhubService>();

            services.Configure<TradeOptions>(configuration.GetSection("TradingOptions"));

            services.AddScoped<IStockService, StockService>();

            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
