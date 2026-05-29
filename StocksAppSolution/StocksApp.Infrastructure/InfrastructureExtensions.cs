using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.HttpClientAbstractions;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.Infrastructure.HttpClients;
using StocksApp.Infrastructure.Repositories;
using StocksApp.Infrastructure.Services;
using StocksApp.Infrastructure.WebSocketClients;

namespace StocksApp.Infrastructure
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IFinnHubHttpClient, FinnhubHttpClient>();
            
            services.AddScoped<IOrderRepository, OrderRepository>();
            
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddTransient<IPasswordHasher, BCryptPasswordHasher>();

            services.AddSingleton<IFinnhubWebSocketClient, FinnhubWebSocketClient>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
    }
}
