using System.Threading.Channels;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.PriceFeed.Registries;

namespace StocksApp.PriceFeed.IoC
{
    public static class PriceFeedExtensions
    {
        public static IServiceCollection AddPriceFeedServices(this IServiceCollection services)
        {
            services.AddSingleton(Channel.CreateBounded<PriceUpdateMessage>(new BoundedChannelOptions(10_000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = true
            }));

            services.AddSingleton(sp => sp.GetRequiredService<Channel<PriceUpdateMessage>>().Reader);
            services.AddSingleton(sp => sp.GetRequiredService<Channel<PriceUpdateMessage>>().Writer);
            services.AddSingleton<ISymbolRegistry, SymbolRegistry>();

            services.AddHostedService<FinnhubIngestionService>();
            return services;
        }
    }
}
