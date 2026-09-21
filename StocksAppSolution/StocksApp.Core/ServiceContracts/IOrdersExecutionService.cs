using StocksApp.Domain.Events;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// Executes orders and persists their outcome.
    /// </summary>
    public interface IOrdersExecutionService
    {
        Task ExecuteSellOrdersAsync(IReadOnlyCollection<SellOrderCreatedCommand> orders,CancellationToken cancellationToken);
    }
}
