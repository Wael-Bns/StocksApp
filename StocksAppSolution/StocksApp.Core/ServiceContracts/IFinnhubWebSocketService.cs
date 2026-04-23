namespace StocksApp.Core.ServiceContracts
{
    public interface IFinnhubWebSocketService : IDisposable
    {
        /// <summary>
        /// Event triggered when a new message is received from the Finnhub WebSocket.
        /// </summary>
        event EventHandler<string>? OnMessageReceived;

        /// <summary>
        /// Connects to the Finnhub WebSocket server.
        /// </summary>
        Task ConnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Subscribes to real-time trades for a specific stock symbol.
        /// </summary>
        Task SubscribeAsync(string stockSymbol, CancellationToken cancellationToken = default);

        /// <summary>
        /// Disconnects from the Finnhub WebSocket server.
        /// </summary>
        Task DisconnectAsync(CancellationToken cancellationToken = default);
    }
}