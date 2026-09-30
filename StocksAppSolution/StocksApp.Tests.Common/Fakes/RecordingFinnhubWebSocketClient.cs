using System.Collections.Concurrent;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;

namespace StocksApp.IntegrationsTests.Fakes
{
    public sealed class RecordingFinnhubWebSocketClient : IFinnhubWebSocketClient
    {
        public event Func<IReadOnlyCollection<PriceUpdateMessage>, Task>? OnPriceUpdatesReceived;

        public ConcurrentBag<string> SubscribeCalls { get; } = new();
        public ConcurrentBag<string> UnsubscribeCalls { get; } = new();
        public ConcurrentQueue<DateTime> ConnectCalls { get; } = new();
        public ConcurrentQueue<DateTime> DisconnectCalls { get; } = new();

        public Func<string, Task>? OnSubscribe { get; set; }

        /// <summary>
        /// Queue of behaviors for successive ReceiveLoopAsync calls. Each call dequeues one;
        /// once empty, ReceiveLoopAsync blocks until the passed CancellationToken is cancelled
        /// (mirrors the real client's behavior of running until the socket closes or ct fires).
        /// </summary>
        public ConcurrentQueue<Func<CancellationToken, Task>> ReceiveLoopBehaviors { get; } = new();

        public Task ConnectAsync(CancellationToken ct = default)
        {
            ConnectCalls.Enqueue(DateTime.UtcNow);
            return Task.CompletedTask;
        }

        public async Task ReceiveLoopAsync(CancellationToken ct = default)
        {
            if (ReceiveLoopBehaviors.TryDequeue(out var behavior))
            {
                await behavior(ct);
                return;
            }

            // default: behave like an open, healthy connection — block until cancelled
            var tcs = new TaskCompletionSource();
            await using var reg = ct.Register(() => tcs.TrySetCanceled(ct));
            await tcs.Task;
        }

        public Task DisconnectAsync(CancellationToken ct = default)
        {
            DisconnectCalls.Enqueue(DateTime.UtcNow);
            return Task.CompletedTask;
        }

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

        /// <summary>Simulates OnPriceUpdatesReceived firing, as the real client would from its receive loop.</summary>
        public Task RaiseUpdatesAsync(IReadOnlyCollection<PriceUpdateMessage> updates) =>
            OnPriceUpdatesReceived?.Invoke(updates) ?? Task.CompletedTask;

        // ---------- behavior helpers ----------

        public static Func<CancellationToken, Task> ThrowsImmediately(Exception ex) =>
            _ => throw ex;

        public static Func<CancellationToken, Task> ReturnsAfter(TimeSpan delay) =>
            async ct => await Task.Delay(delay, ct);

        public static Func<CancellationToken, Task> RunsUntilCancelled() =>
            async ct =>
            {
                var tcs = new TaskCompletionSource();
                await using var reg = ct.Register(() => tcs.TrySetCanceled(ct));
                await tcs.Task;
            };
    }
}