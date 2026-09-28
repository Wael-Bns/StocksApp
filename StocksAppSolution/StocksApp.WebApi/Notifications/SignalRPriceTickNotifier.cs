using Microsoft.AspNetCore.SignalR;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Events;
using StocksApp.WebApi.Hubs;

namespace StocksApp.WebApi.Notifications
{
    public sealed class SignalRPriceTickNotifier : IPriceTickNotifier
    {
        private const string ReceivePriceUpdateMethod = "ReceivePriceUpdate";

        private readonly IHubContext<StocksHub> _stocksHub;
        private readonly ILogger<SignalRPriceTickNotifier> _logger;

        public SignalRPriceTickNotifier(IHubContext<StocksHub> stocksHub, ILogger<SignalRPriceTickNotifier> logger)
        {
            _stocksHub = stocksHub;
            _logger = logger;
        }

        public async Task NotifyAsync(IPriceTickPublished priceTick, CancellationToken ct = default)
        {
            if (priceTick is null) return;

            var symbol = Normalize(priceTick.StockSymbol);

            try
            {
                await _stocksHub.Clients.Group(symbol)
                    .SendAsync(ReceivePriceUpdateMethod, symbol, priceTick.Price, cancellationToken: ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast price update for {Symbol}", symbol);
            }
        }

        private static string Normalize(string symbol) => symbol.Trim().ToUpperInvariant();
    }
}