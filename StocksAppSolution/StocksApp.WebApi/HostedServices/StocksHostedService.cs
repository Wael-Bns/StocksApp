using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using StocksApp.Core.WebSocketClientAstractions;
using StocksApp.WebApi.Hubs;
using StocksApp.WebApi.Models;

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
        private async Task NotifySubscribers(string message)
        {
            try
            {
                FinnhubTradeMessage? tradeMessage = JsonSerializer.Deserialize<FinnhubTradeMessage>(message);
                if (tradeMessage?.Data != null)
                {
                    foreach (var trade in tradeMessage.Data)
                    {
                        await _stocksHub.Clients.Group(trade.Symbol)
                            .SendAsync("ReceivePriceUpdate", trade.Price);
                    }
                }

            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize message: {Message}", message);
            }
        }
    }
}
