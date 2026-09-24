using StocksApp.Domain.Events;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// Subscribes and unsubscribes to real time price updates of a specific symbol pushed from the price feed service through the message broker
    /// </summary>
    public interface IPriceFeedSubscriber : IAsyncDisposable
    {
        event Func<IPriceTickPublished, CancellationToken, Task>? OnPriceTick;
        Task SubscribeAsync(string symbol, CancellationToken ct = default);
        Task UnsubscribeAsync(string symbol, CancellationToken ct = default);
    }
}
