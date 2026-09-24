using Microsoft.AspNetCore.SignalR;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.WebApi.Hubs
{
    public class StocksHub : Hub
    {
        private readonly ILogger<StocksHub> _logger;
        private readonly IStockSubscriptionTracker _subscriptionTracker;

        public StocksHub(ILogger<StocksHub> logger, IStockSubscriptionTracker subscriptionTracker)
        {
            _logger = logger;
            _subscriptionTracker = subscriptionTracker;
        }

        public async Task SubscribeToSymbol(string symbol)
        {
            symbol = symbol.Trim().ToUpperInvariant();
            try
            {
                await _subscriptionTracker.AddInterestAsync(Context.ConnectionId, symbol, Context.ConnectionAborted);
                await Groups.AddToGroupAsync(Context.ConnectionId, symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to subscribe connection {ConnectionId} to {Symbol}", Context.ConnectionId, symbol);
                throw;
            }
        }

        public async Task UnsubscribeFromSymbol(string symbol)
        {
            symbol = symbol.Trim().ToUpperInvariant();
            await _subscriptionTracker.RemoveInterestAsync(Context.ConnectionId, symbol, Context.ConnectionAborted);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol);
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to {HubName}: {ConnectionId}", nameof(StocksHub), Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected from {HubName}: {ConnectionId}", nameof(StocksHub), Context.ConnectionId);

            var symbols = await _subscriptionTracker.RemoveConnectionAsync(Context.ConnectionId);
            foreach (var symbol in symbols)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}