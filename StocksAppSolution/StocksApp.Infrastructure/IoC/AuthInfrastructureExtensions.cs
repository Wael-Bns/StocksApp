using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.ServiceContracts;
using StocksApp.Infrastructure.Services;

namespace StocksApp.Infrastructure.IoC
{
    public static class AuthInfrastructureExtensions
    {
        public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<IPasswordHasher, BCryptPasswordHasher>();

            return services;
        }
    }
}
