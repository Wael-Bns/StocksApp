using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.HttpClientAbstractions;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.Infrastructure.HttpClients;
using StocksApp.Infrastructure.Options;
using StocksApp.Infrastructure.WebSocketClients;

namespace StocksApp.Infrastructure.IoC
{
    public static class FinnhubClientExtensions
    {
        public static IServiceCollection AddFinnhubClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FinnhubOptions>(options =>
            {
                options.ApiKey = configuration["FinnhubApiKey"] ?? string.Empty;
            });

            services.AddHttpClient<IFinnHubHttpClient, FinnhubHttpClient>(options =>
            {
                options.BaseAddress = new Uri("https://finnhub.io/api/v1/");
            });

            services.AddSingleton<IFinnhubWebSocketClient, FinnhubWebSocketClient>();
            services.AddSingleton<ISymbolRegistry, SymbolRegistry>();

            return services;
        }
    }
}
