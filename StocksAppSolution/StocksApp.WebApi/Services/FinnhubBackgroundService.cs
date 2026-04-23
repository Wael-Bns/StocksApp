using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StocksApp.Core.Domain.MessageQueueContracts;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.WebApi.Services
{
    public class FinnhubBackgroundService : BackgroundService
    {
        private readonly IFinnhubWebSocketService _webSocketService;
        private readonly ILogger<FinnhubBackgroundService> _logger;
        private readonly ISellOrdersStore _sellOrderStore;
        private readonly IServiceScopeFactory _scopeFactory;

        public FinnhubBackgroundService(
            IFinnhubWebSocketService webSocketService,
            ILogger<FinnhubBackgroundService> logger,
            ISellOrdersStore sellOrderStore,
            IServiceScopeFactory scopeFactory)
        {
            _webSocketService = webSocketService;
            _logger = logger;
            _sellOrderStore = sellOrderStore;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Finnhub WebSocket listener...");

            // Use a lambda to avoid 'async void' crash risks. Catch everything locally.
            _webSocketService.OnMessageReceived += async (sender, message) =>
            {
                try { await ProcessMessageSafeAsync(message); }
                catch (Exception ex) { _logger.LogError(ex, "Fatal error in message stream"); }
            };

            await _webSocketService.ConnectAsync(stoppingToken);

            var stockSymbols = await _sellOrderStore.GetAvailableStockSymbols();
            foreach (var symbol in stockSymbols)
            {
                await _webSocketService.SubscribeAsync(symbol, stoppingToken);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            await _webSocketService.DisconnectAsync(stoppingToken);
        }

        private async Task ProcessMessageSafeAsync(string message)
        {
            using var document = JsonDocument.Parse(message);
            var root = document.RootElement;
            // Only process trade messages
            if (root.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "trade")
            {
                // Each message can contain multiple trades, so we loop through them
                var dataArray = root.GetProperty("data");
                foreach (var trade in dataArray.EnumerateArray())
                {
                    var symbol = trade.GetProperty("s").GetString();
                    var price = trade.GetProperty("p").GetDouble();
                    // Get valid sell orders for this symbol and price
                    var validSellOrders = await _sellOrderStore.GetValidSellOrders(symbol!, price);
                    if (validSellOrders.Count > 0)
                    {
                        _logger.LogInformation("Found {Count} valid sell orders for {Symbol}", validSellOrders.Count, symbol);

                        // CREATE A SCOPE for database operations!
                        using var scope = _scopeFactory.CreateScope();
                        var tradeExecutionService = scope.ServiceProvider.GetRequiredService<ITradeExecutionService>();

                        foreach (var sellOrder in validSellOrders)
                        {
                            await tradeExecutionService.FinalizeSellOrderAsync(sellOrder);
                        }
                    }
                }
            }
        }
    }
}