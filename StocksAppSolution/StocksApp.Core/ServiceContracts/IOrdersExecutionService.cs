using StocksApp.Domain.Events;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// Responsible for executing sell orders based on the provided requests.
    /// </summary>
    public interface IOrdersExecutionService
    {
        Task ExecuteSellOrdersAsync(IReadOnlyCollection<SellOrderCreatedCommand> orders,CancellationToken cancellationToken);
    }
}
