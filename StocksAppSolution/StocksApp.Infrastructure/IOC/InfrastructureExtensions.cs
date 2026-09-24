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
            // Http clients
            services.AddHttpClient<IFinnHubHttpClient, FinnhubHttpClient>(options =>
            {
                options.BaseAddress = new Uri("https://finnhub.io/api/v1/");
            });

            // Configurations
            services.Configure<FinnhubOptions>(options =>
            {
                options.ApiKey = configuration["FinnhubApiKey"] ?? string.Empty;
            });

            services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

            // Transient
            services.AddTransient<IPasswordHasher, BCryptPasswordHasher>();

            // Scoped 
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IOutboxRepository, OutboxRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IStockService, StockService>();

            //Singleton
            services.AddSingleton<ISymbolRegistry, SymbolRegistry>();
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
