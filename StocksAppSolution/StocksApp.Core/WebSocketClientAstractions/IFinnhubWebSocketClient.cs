
namespace StocksApp.Core.WebSocketClientAstractions
{
    public interface IFinnhubWebSocketClient
    {
        event Func<string, Task>? OnMessageReceived;

        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task ReceiveAsync(CancellationToken cancellationToken = default);
        Task SubscribeAsync(string symbol, CancellationToken cancellationToken = default);
        Task UnsubscribeAsync(string symbol, CancellationToken cancellationToken = default);
    }
}