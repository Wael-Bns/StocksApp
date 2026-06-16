using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.DTO.StockDTO;

namespace StocksApp.Core.ServiceContracts
{
    public interface IStockService
    {
        /// <summary>
        /// Create a Buy order with the given informations in buyOrderRequest
        /// </summary>
        /// <param name="buyOrderRequest"></param>
        /// <param name="userId">The ID of the user creating the buy order.</param>
        /// <returns>Returns the created object</returns>
        Task<BuyOrderResponse> CreateBuyOrder(BuyOrderAddRequest? buyOrderRequest, Guid userId);
        /// <summary>
        /// Create a Sell order with the given informations in sellOrderRequest
        /// </summary>
        /// <param name="sellOrderRequest"></param>
        /// <param name="userId">The ID of the user creating the sell order.</param>
        /// <returns>Returns the created object</returns>
        Task<SellOrderResponse> CreateSellOrder(SellOrderAddRequest? sellOrderRequest, Guid userId);
        /// <summary>
        /// Get the list of buy orders saved in the database for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user whose buy orders are being fetched.</param>
        /// <returns>List of buy orders for the specified user.</returns>
        Task<List<BuyOrderResponse>> GetBuyOrdersByUser(Guid userId);
        /// <summary>
        /// Get the list of sell orders saved in the database for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user whose sell orders are being fetched.</param>
        /// <returns>List of sell orders for the specified user.</returns>
        Task<List<SellOrderResponse>> GetSellOrdersByUser(Guid userId);
        /// <summary>
        /// Returns the stock informations based on the provided stock symbol .
        /// </summary>
        /// <param name="stockSymbol">Symbol based on which informations are fetched .</param>
        /// <returns>Stock informations</returns>
        Task<StockInformations> GetStockInformations(string stockSymbol);
    }
}
