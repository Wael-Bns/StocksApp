using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Infrastructure.Repositories;

namespace StocksApp.Infrastructure.IoC
{
    public static class PersistenceExtensions
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            if (!environment.IsEnvironment("Test"))
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("No connection string was provided");
                services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
            }

            return services;
        }
    }
}
