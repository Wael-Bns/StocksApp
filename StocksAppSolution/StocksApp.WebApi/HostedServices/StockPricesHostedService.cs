using Microsoft.AspNetCore.SignalR;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.WebApi.Hubs;

namespace StocksApp.WebApi.HostedServices
{
    public class StockPricesHostedService : BackgroundService
    {
        private readonly ILogger<StockPricesHostedService> _logger;
        private readonly IHubContext<StocksHub> _stocksHub;
        private readonly IFinnhubWebSocketClient _finnhubWebSocketClient;
        public StockPricesHostedService(ILogger<StockPricesHostedService> logger, IHubContext<StocksHub> stocksHub, IFinnhubWebSocketClient finnhubWebSocketClient)
        {
            _logger = logger;
            _stocksHub = stocksHub;
            _finnhubWebSocketClient = finnhubWebSocketClient;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting {ServiceName}...", nameof(StockPricesHostedService));
                await _finnhubWebSocketClient.ConnectAsync(stoppingToken);

                _finnhubWebSocketClient.OnMessageReceived += async (message) => await NotifySubscribers(message);
            
                await _finnhubWebSocketClient.ReceiveAsync(stoppingToken);
            }
            finally
            {
                _finnhubWebSocketClient.OnMessageReceived -= async (message) => await NotifySubscribers(message);
                await _finnhubWebSocketClient.DisconnectAsync(CancellationToken.None);
                _logger.LogInformation("{ServiceName} stopped.", nameof(StockPricesHostedService));
            }
        }
        private async Task NotifySubscribers(IReadOnlyCollection<PriceUpdateMessage> priceUpdates)
        {
            if (priceUpdates != null)
            {
                foreach (var trade in priceUpdates)
                {
                    await _stocksHub.Clients.Group(trade.StockSymbol)
                        .SendAsync("ReceivePriceUpdate", trade.Price);
                }
            }
        }
    }
}
