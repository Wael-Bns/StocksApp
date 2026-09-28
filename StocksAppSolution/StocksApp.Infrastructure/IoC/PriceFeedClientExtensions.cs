using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.ServiceContracts;
using StocksApp.Infrastructure.Options;
using StocksApp.Infrastructure.Services;

namespace StocksApp.Infrastructure.IoC
{
    public static class PriceFeedClientExtensions
    {
        public static IServiceCollection AddPriceFeedSubscriber(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PriceFeedClientOptions>(configuration.GetSection(PriceFeedClientOptions.SectionName));
            services.AddSingleton<IPriceFeedSubscriber, PriceFeedSubscriber>();
            
            return services;
        }
    }
}
