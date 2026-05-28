using StocksApp.Core.DTO.StockDTO;

namespace StocksApp.OrdersWorker.ServiceContracts
{
    /// <summary>
    /// Responsible for executing the orders based on the updated stock prices.
    /// </summary>
    public interface IPriceUpdateOrderProcessor
    {
        Task EnqueueMessageAsync(PriceUpdateMessage priceUpdateMessage);
        Task StartAsync(CancellationToken cancellationToken = default);
    }
}
