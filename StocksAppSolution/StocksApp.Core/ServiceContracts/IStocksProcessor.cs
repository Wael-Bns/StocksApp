using StocksApp.Core.DTO.StockDTO;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// Responsible for executing the orders based on the updated stock prices.
    /// </summary>
    public interface IStocksProcessor
    {
        Task EnqueueMessageAsync(PriceUpdateMessage priceUpdateMessage);
        Task StartAsync(CancellationToken cancellationToken = default);
    }
}
