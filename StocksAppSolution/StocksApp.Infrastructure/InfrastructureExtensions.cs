using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.HttpClientAbstractions;
using StocksApp.Infrastructure.Repositories;
using StocksApp.Infrastructure.Services;
using StocksApp.Core.ServiceContracts;
using StocksApp.Infrastructure.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using StocksApp.Core.Services;

namespace StocksApp.Infrastructure
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddHttpClient<IFinnHubHttpClient, FinnhubHttpClient>(options =>
            {
                options.BaseAddress = new Uri("https://finnhub.io/api/v1/");
            });

            services.Configure<FinnhubOptions>(options =>
            {
                options.ApiKey = configuration["FinnhubApiKey"] ?? throw new ArgumentNullException("FinnhubApiKey configuration is missing.");
            });
            
            services.AddScoped<IOrderRepository, OrderRepository>();
            
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddTransient<IPasswordHasher, BCryptPasswordHasher>();

            services.AddScoped<IStockService, StockService>();

            if(!environment.IsEnvironment("Test"))
            {
                string connectionStringTemplate = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("No connection string was provided");
                string connectionString = connectionStringTemplate
                                            .Replace("$POSTGRES_HOST", Environment.GetEnvironmentVariable("POSTGRES_HOST"))
                                            .Replace("$POSTGRES_PORT", Environment.GetEnvironmentVariable("POSTGRES_PORT"))
                                            .Replace("$POSTGRES_USER", Environment.GetEnvironmentVariable("POSTGRES_USER"))
                                            .Replace("$POSTGRES_PASSWORD", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"))
                                            .Replace("$POSTGRES_DB", Environment.GetEnvironmentVariable("POSTGRES_DB"));
           
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(connectionString);
                });
            }

            return services;
        }
    }
}
