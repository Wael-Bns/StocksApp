using StocksApp.Core.DTO.Commands;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// Responsible for executing sell orders based on the provided requests.
    /// </summary>
    public interface IOrdersExecutionService
    {
        Task ExecuteSellOrdersAsync(IReadOnlyCollection<ExecuteSellOrderCommand> orders,CancellationToken cancellationToken);
    }
}
