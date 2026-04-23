using System.Collections.Immutable;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.Domain.MessageQueueContracts
{
    /// <summary>
    /// An abstraction of the store containing sell orders that are waiting to be executed.
    /// </summary>
    public interface ISellOrdersStore
    {
        /// <summary>
        /// Adds a sell order in the store
        /// </summary>
        /// <param name="sellOrder"></param>
        /// <returns></returns>
        Task AddSellOrder(SellOrder sellOrder);
        /// <summary>
        /// Returns a list of sell orders of a given stock that have a price less than or equal to the given validPrice
        /// </summary>
        /// <param name="stockSymbol">The stock on which the comparision will occur</param>
        /// <param name="validPrice">Sell orders having a price under this price will be executed</param>
        /// <returns>List of valid sell orders</returns>
        Task<List<SellOrder>> GetValidSellOrders(string stockSymbol, double validPrice);
        /// <summary>
        /// Returns a list of stock symbols that are available for trading
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetAvailableStockSymbols();
    }
}
