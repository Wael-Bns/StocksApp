using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;

namespace StocksApp.Core.IoC
{
    public static class StockCoreExtensions
    {
        public static IServiceCollection AddStockCore(this IServiceCollection services)
        {
            services.AddScoped<IStockService, StockService>();
            return services;
        }
    }
}
