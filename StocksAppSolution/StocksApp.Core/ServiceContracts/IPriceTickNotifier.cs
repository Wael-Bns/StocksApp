using StocksApp.Domain.Events;

namespace StocksApp.Core.ServiceContracts
{
    public interface IPriceTickNotifier
    {
        Task NotifyAsync(IPriceTickPublished priceTick, CancellationToken ct = default);
    }
}
