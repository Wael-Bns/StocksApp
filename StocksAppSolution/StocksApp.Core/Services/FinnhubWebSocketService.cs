using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class FinnhubWebSocketService : IFinnhubWebSocketService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FinnhubWebSocketService> _logger;
        private ClientWebSocket _webSocket;
        private CancellationTokenSource? _receiveCts;

        public event EventHandler<string>? OnMessageReceived;

        public FinnhubWebSocketService(IConfiguration configuration, ILogger<FinnhubWebSocketService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _webSocket = new ClientWebSocket();
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (_webSocket.State == WebSocketState.Open)
                return;

            string? token = _configuration["FinnhubToken"];
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("FinnhubToken is missing from configuration.");
            }

            Uri serverUri = new Uri($"wss://ws.finnhub.io?token={token}");

            try
            {
                // Re-initialize if previously closed/aborted
                if (_webSocket.State is WebSocketState.Closed or WebSocketState.Aborted)
                {
                    _webSocket.Dispose();
                    _webSocket = new ClientWebSocket();
                }

                await _webSocket.ConnectAsync(serverUri, cancellationToken);
                _logger.LogInformation("Connected to Finnhub WebSocket.");

                // Start the background listening loop
                _receiveCts = new CancellationTokenSource();
                _ = ReceiveLoopAsync(_receiveCts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to Finnhub WebSocket");
                throw;
            }
        }

        public async Task SubscribeAsync(string stockSymbol, CancellationToken cancellationToken = default)
        {
            if (_webSocket.State != WebSocketState.Open)
            {
                _logger.LogWarning("WebSocket is not open. Cannot subscribe to {Symbol}.", stockSymbol);
                return;
            }

            var request = new
            {
                type = "subscribe",
                symbol = stockSymbol
            };

            string jsonMessage = JsonSerializer.Serialize(request);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonMessage);

            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            _logger.LogInformation("Subscribed to {Symbol}", stockSymbol);
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                _receiveCts?.Cancel();
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", cancellationToken);
                _logger.LogInformation("Disconnected from Finnhub WebSocket.");
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    WebSocketReceiveResult result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync(cancellationToken);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        // Fire the event when a message is received
                        OnMessageReceived?.Invoke(this, message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("WebSocket receive loop cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving message from WebSocket.");
            }
        }

        public void Dispose()
        {
            _receiveCts?.Cancel();
            _receiveCts?.Dispose();

            if (_webSocket.State == WebSocketState.Open)
            {
                _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing client", CancellationToken.None).GetAwaiter().GetResult();
            }
            _webSocket.Dispose();
        }
    }
}