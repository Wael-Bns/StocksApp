using StocksApp.Core.DTO.SellOrderDTO;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// This class is responsible for executing orders .
    /// </summary>
    public interface IOrdersExecutor
    {
        /// <summary>
        /// Executes the sell orders based on the provided market price and returns a list of SellOrderResponse objects containing the details of the executed sell orders.
        /// </summary>
        /// <param name="marketPrice">The price under which the sell orders should be executed.</param>
        /// <param name="stockSymbol">The stock symbol for which to execute sell orders</param>
        /// <returns>A list of SellOrderResponse objects representing the executed sell orders, or null if no orders were executed.</returns>
        Task<List<SellOrderResponse>?> ExecuteSellOrders(string stockSymbol,double marketPrice);
    }
}
