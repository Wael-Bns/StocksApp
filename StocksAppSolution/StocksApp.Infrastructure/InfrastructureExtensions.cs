using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.HttpClientAbstractions;
using StocksApp.Infrastructure.Repositories;
using StocksApp.Infrastructure.Services;
using StocksApp.Core.ServiceContracts;

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

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
    }
}
