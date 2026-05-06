using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.HttpClientAbstractions;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;

namespace StocksApp.Core
{
    public static class CoreExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<IStockService, StockService>();

            services.AddScoped<IUserService, UserService>();

            services.AddTransient<IJwtService, JwtService>();

            return services;
        }
    }
}
