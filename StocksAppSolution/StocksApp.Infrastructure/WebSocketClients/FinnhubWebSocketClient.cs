using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.Infrastructure.Models;

namespace StocksApp.Infrastructure.WebSocketClients
{
    public class FinnhubWebSocketClient : IFinnhubWebSocketClient
    {
        private readonly ClientWebSocket _socket = new ClientWebSocket();
        private readonly IConfiguration _configuration;
        private readonly ILogger<FinnhubWebSocketClient> _logger;
        public event Func<IReadOnlyCollection<PriceUpdateMessage>, Task>? OnMessageReceived;
        public FinnhubWebSocketClient(IConfiguration configuration, ILogger<FinnhubWebSocketClient> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            string token = _configuration["FinnhubApiKey"]!;
            var uri = new Uri($"wss://ws.finnhub.io?token={token}");
            await _socket.ConnectAsync(uri, cancellationToken);
            _logger.LogInformation("Connected to finnhub websocket .");
        }
        public async Task SubscribeAsync(string symbol, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                type = "subscribe",
                symbol = symbol
            };
            string jsonPayload = JsonSerializer.Serialize(payload);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonPayload);
            await _socket.SendAsync(
                    bytes,
                    WebSocketMessageType.Text,
                    true, cancellationToken);
            _logger.LogInformation("Subscribed to stock symbol: {Symbol}", symbol);
        }
        public async Task ReceiveAsync(CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[4096];

            while (_socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                using var messageStream = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await _socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                        _logger.LogInformation("WebSocket connection closed.");
                        return;
                    }

                    messageStream.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                string message = Encoding.UTF8.GetString(messageStream.ToArray());
                FinnhubTradeMessage? tradeMessage = JsonSerializer.Deserialize<FinnhubTradeMessage>(message);
                IReadOnlyCollection<PriceUpdateMessage>? priceUpdates = tradeMessage?.ToPriceUpdateMessageList();
                
                if (priceUpdates != null && OnMessageReceived != null)
                {
                    await OnMessageReceived.Invoke(priceUpdates);
                }
            }
        }
        public async Task UnsubscribeAsync(string symbol, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                type = "unsubscribe",
                symbol = symbol
            };
            string jsonPayload = JsonSerializer.Serialize(payload);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonPayload);
            await _socket.SendAsync(
                    bytes,
                    WebSocketMessageType.Text,
                    true, cancellationToken);
            _logger.LogInformation("Unsubscribed from stock symbol: {Symbol}", symbol);
        }
    }
}
