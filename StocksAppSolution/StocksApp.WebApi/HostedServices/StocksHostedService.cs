using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.WebApi.Hubs;

namespace StocksApp.WebApi.HostedServices
{
    public class StocksHostedService : BackgroundService
    {
        private readonly ILogger<StocksHostedService> _logger;
        private readonly IHubContext<StocksHub> _stocksHub;
        private readonly IFinnhubWebSocketClient _finnhubWebSocketClient;
        public StocksHostedService(ILogger<StocksHostedService> logger, IHubContext<StocksHub> stocksHub, IFinnhubWebSocketClient finnhubWebSocketClient)
        {
            _logger = logger;
            _stocksHub = stocksHub;
            _finnhubWebSocketClient = finnhubWebSocketClient;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting {ServiceName}...", nameof(StocksHostedService));
            await _finnhubWebSocketClient.ConnectAsync(stoppingToken);

            _finnhubWebSocketClient.OnMessageReceived += async (message) => await NotifySubscribers(message);
            
            await _finnhubWebSocketClient.ReceiveAsync(stoppingToken);
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
