using System.Threading.Channels;
using StocksApp.Core.Diagnostics;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Infrastructure.Repositories;
using StocksApp.Infrastructure.Services;
using StocksApp.PriceFeed.BackgroundServices;
using StocksApp.PriceFeed.Diagnostics;
using StocksApp.PriceFeed.Options;

namespace StocksApp.PriceFeed.IoC
{
    public static class PriceFeedExtensions
    {
        public static IServiceCollection AddPriceFeedServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PriceFeedIngestionOptions>(configuration.GetSection(PriceFeedIngestionOptions.SectionName));

            services.AddScoped<ICandleRepository, CandleRepository>();
            services.AddSingleton<ICandleStore, ScopedCandleStore>();
            services.AddSingleton<IOhlcBarAggregator, OhlcBarAggregator>();

            services.AddSingleton<PriceFeedMetrics>();
            services.AddSingleton<IPriceFeedMetrics>(sp => sp.GetRequiredService<PriceFeedMetrics>());
            services.AddSingleton<ICandleMetrics>(sp => sp.GetRequiredService<PriceFeedMetrics>());
            services.AddSingleton(sp =>
            {
                var metrics = sp.GetRequiredService<IPriceFeedMetrics>();
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("PriceFeed.Channel");
                long dropCount = 0;

                return Channel.CreateBounded<PriceUpdateMessage>(
                    new BoundedChannelOptions(10_000)
                    {
                        FullMode = BoundedChannelFullMode.DropOldest,
                        SingleReader = true,
                        SingleWriter = true
                    },
                    itemDropped: dropped =>
                    {
                        metrics.TickDropped();
                        // log 1st and every 1000th drop so a stall doesn't flood the logs
                        var n = Interlocked.Increment(ref dropCount);
                        if (n == 1 || n % 1000 == 0)
                            logger.LogWarning("Tick channel full: {Count} ticks dropped so far (latest {Symbol})",
                                n, dropped.StockSymbol);
                    });
            });


            services.AddSingleton(sp => sp.GetRequiredService<Channel<PriceUpdateMessage>>().Reader);
            services.AddSingleton(sp => sp.GetRequiredService<Channel<PriceUpdateMessage>>().Writer);

            services.AddHostedService<TickPublisherService>();
            services.AddHostedService<FinnhubIngestionService>();

            return services;
        }
    }
}