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

            services.AddScoped<IStockService, StockService>();

            services.AddScoped<IUserService, UserService>();

            services.AddTransient<ITokenService, JwtService>();

            services.AddTransient<IAuthService, AuthService>();

            services.AddScoped<IOrdersService, OrdersService>();

            services.AddSingleton<ISubscriptionsManager, SubscriptionsManager>();
            // Configure Options pattern 
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

            services.Configure<RefreshTokenOptions>(configuration.GetSection(RefreshTokenOptions.SectionName));

            return services;
        }
    }
}
