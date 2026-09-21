using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;

namespace StocksApp.IntegrationsTests.Fakes
{
    public class FakeFinnhubWebSocketClient : IFinnhubWebSocketClient
    {
        public event Func<IReadOnlyCollection<PriceUpdateMessage>, Task>? OnPriceUpdatesReceived;

        public Task ConnectAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task ReceiveLoopAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task DisconnectAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task SubscribeAsync(string symbol, CancellationToken cancellationToken = default) => Task.CompletedTask;
            
        public Task UnsubscribeAsync(string symbol, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}