
using StocksApp.Core.DTO.StockDTO;

namespace StocksApp.Core.WebSocketClientAbstractions
{
    public interface IFinnhubWebSocketClient
    {
        event Func<IReadOnlyCollection<PriceUpdateMessage>, Task>? OnMessageReceived;
        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task ReceiveAsync(CancellationToken cancellationToken = default);
        Task SubscribeAsync(string symbol, CancellationToken cancellationToken = default);
        Task UnsubscribeAsync(string symbol, CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);
    }
}