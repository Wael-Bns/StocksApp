using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Core.HttpClientAbstractions;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.Infrastructure.HttpClients;
using StocksApp.Infrastructure.Repositories;
using StocksApp.Infrastructure.Services;
using StocksApp.Infrastructure.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using StocksApp.Core.Services;
using StocksApp.Infrastructure.WebSocketClients;

namespace StocksApp.Infrastructure.IoC
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.AddHttpClient<IFinnHubHttpClient, FinnhubHttpClient>(options =>
            {
                options.BaseAddress = new Uri("https://finnhub.io/api/v1/");
            });

            services.Configure<FinnhubOptions>(options =>
            {
                options.ApiKey = configuration["FinnhubApiKey"] ?? string.Empty;
            });

            services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

            services.AddScoped<IOrderRepository, OrderRepository>();

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddTransient<IPasswordHasher, BCryptPasswordHasher>();

            services.AddScoped<IStockService, StockService>();

            services.AddSingleton<IFinnhubWebSocketClient, FinnhubWebSocketClient>();

            if(!environment.IsEnvironment("Test"))
            {
                string connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("No connection string was provided");
                
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(connectionString);
                });
            }

            return services;
        }
    }
}
