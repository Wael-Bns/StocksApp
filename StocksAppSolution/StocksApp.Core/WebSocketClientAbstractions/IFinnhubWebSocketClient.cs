
using StocksApp.Core.DTO.StockDTO;

namespace StocksApp.Core.WebSocketClientAbstractions
{
    /// <summary>
    ///     Communicates with the Finnhub real-time feed and exposes its data to the rest of the app.
    /// </summary>
    public interface IFinnhubWebSocketClient
    {
        event Func<IReadOnlyCollection<PriceUpdateMessage>, Task>? OnPriceUpdatesReceived;
        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task ReceiveLoopAsync(CancellationToken cancellationToken = default);
        Task SubscribeAsync(string symbol, CancellationToken cancellationToken = default);
        Task UnsubscribeAsync(string symbol, CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);
    }
}