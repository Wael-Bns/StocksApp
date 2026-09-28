using System.Net.WebSockets;
using System.Text.Json;
using MassTransit.Internals.GraphValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.Infrastructure.Models;
using StocksApp.Infrastructure.Options;

namespace StocksApp.Infrastructure.WebSocketClients
{
    public sealed class FinnhubWebSocketClient : IFinnhubWebSocketClient, IDisposable
    {
        private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(5);

        private readonly FinnhubOptions _finnhubOptions;
        private readonly ILogger<FinnhubWebSocketClient> _logger;
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private volatile ClientWebSocket? _socket;

        public event Func<IReadOnlyCollection<PriceUpdateMessage>, Task>? OnPriceUpdatesReceived;

        public FinnhubWebSocketClient(
          ILogger<FinnhubWebSocketClient> logger,
          IOptions<FinnhubOptions> finnhubOptions)
        {
            _logger = logger;
            _finnhubOptions = finnhubOptions.Value;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            Interlocked.Exchange(ref _socket, null)?.Dispose();

            var socket = new ClientWebSocket();
            try
            {
                var uri = new Uri($"wss://ws.finnhub.io?token={_finnhubOptions.ApiKey}");
                await socket.ConnectAsync(uri, cancellationToken);
            }
            catch
            {
                socket.Dispose();
                throw;
            }

            _socket = socket;
            _logger.LogInformation("Connected to Finnhub websocket.");
        }

        public Task SubscribeAsync(string symbol, CancellationToken cancellationToken = default)
          => SendAsync("subscribe", symbol, cancellationToken);

        public Task UnsubscribeAsync(string symbol, CancellationToken cancellationToken = default)
          => SendAsync("unsubscribe", symbol, cancellationToken);

        private async Task SendAsync(string type, string symbol, CancellationToken ct)
        {
            var socket = _socket;
            if (socket is null || socket.State != WebSocketState.Open)
                throw new InvalidOperationException("Finnhub socket is not open.");

            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(new { type, symbol });

            await _sendLock.WaitAsync(ct);
            try
            {
                // state can change while waiting for the lock
                if (socket.State != WebSocketState.Open)
                    throw new InvalidOperationException("Finnhub socket closed while waiting to send.");

                await socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
            }
            finally
            {
                _sendLock.Release();
            }

            _logger.LogInformation("Sent {Type} for {Symbol}", type, symbol);
        }

        public async Task ReceiveLoopAsync(CancellationToken cancellationToken = default)
        {
            var socket = _socket ?? throw new InvalidOperationException("Finnhub socket is not connected.");
            var buffer = new byte[4096];

            while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                using var messageStream = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning("Finnhub closed the WebSocket connection ({Status}).", result.CloseStatus);
                        await CloseQuietlyAsync(socket);
                        return;
                    }

                    messageStream.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                FinnhubTradeMessage? trade;
                try
                {
                    trade = JsonSerializer.Deserialize<FinnhubTradeMessage>(
                      messageStream.GetBuffer().AsSpan(0, (int)messageStream.Length));
                }
                catch (JsonException ex)
                {
                    _logger.LogDebug(ex, "Skipping unparseable Finnhub frame");
                    continue;
                }

                var updates = trade?.ToPriceUpdateMessageList();
                var handler = OnPriceUpdatesReceived;
                if (updates is not null && handler is not null)
                    await handler.Invoke(updates);
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            var socket = _socket;
            if (socket is not null)
                await CloseQuietlyAsync(socket);
        }

        private async Task CloseQuietlyAsync(ClientWebSocket socket)
        {
            if (socket.State is not (WebSocketState.Open or WebSocketState.CloseReceived))
                return;

            using var cts = new CancellationTokenSource(CloseTimeout);
            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cts.Token);
                _logger.LogInformation("Disconnected from Finnhub websocket.");
            }
            catch (Exception ex) when (ex is OperationCanceledException or WebSocketException)
            {
                _logger.LogWarning(ex, "Finnhub socket did not close cleanly; aborting.");
                socket.Abort();
            }
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _socket, null)?.Dispose();
            _sendLock.Dispose();
        }
    }
}