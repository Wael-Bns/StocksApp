using System.Collections.Concurrent;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;

namespace StocksApp.Tests.Common.Fakes
{
    public sealed class RecordingFinnhubWebSocketClient : IFinnhubWebSocketClient
    {
        public event Func<IReadOnlyCollection<PriceUpdateMessage>, Task>? OnPriceUpdatesReceived;

        public ConcurrentBag<string> SubscribeCalls { get; } = new();
        public ConcurrentBag<string> UnsubscribeCalls { get; } = new();
        public Func<string, Task>? OnSubscribe { get; set; }   // hook for failure-injection tests

        public Task ConnectAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task ReceiveLoopAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken ct = default) => Task.CompletedTask;

        public async Task SubscribeAsync(string symbol, CancellationToken ct = default)
        {
            if (OnSubscribe is not null) await OnSubscribe(symbol);
            SubscribeCalls.Add(symbol);
        }

        public Task UnsubscribeAsync(string symbol, CancellationToken ct = default)
        {
            UnsubscribeCalls.Add(symbol);
            return Task.CompletedTask;
        }
    }
}
