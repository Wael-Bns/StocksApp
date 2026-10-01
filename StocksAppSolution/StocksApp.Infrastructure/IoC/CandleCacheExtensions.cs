using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StocksApp.Core.ServiceContracts;
using StocksApp.Infrastructure.Caching;
using StocksApp.Infrastructure.Options;

namespace StocksApp.Infrastructure.IoC
{
    public static class CandleCacheExtensions
    {
        public static IServiceCollection AddCandleCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CandleCacheOptions>(configuration.GetSection(CandleCacheOptions.SectionName));

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<CandleCacheOptions>>().Value;
                return ConnectionMultiplexer.Connect(options.RedisConnectionString);
            });

            services.AddSingleton<ICandleCache, RedisCandleCache>();

            return services;
        }
    }
}