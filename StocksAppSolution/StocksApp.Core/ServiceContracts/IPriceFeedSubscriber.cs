using StocksApp.Domain.Events;

namespace StocksApp.Core.ServiceContracts
{
    public interface IPriceFeedSubscriber : IAsyncDisposable
    {
        event Func<IPriceTickPublished, Task>? OnPriceTick;
        Task SubscribeAsync(string symbol, CancellationToken ct = default);
        Task UnsubscribeAsync(string symbol, CancellationToken ct = default);
    }
}
