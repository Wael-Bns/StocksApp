using StocksApp.Core.DTO;

namespace StocksApp.Core.ServiceContracts
{
    public interface IStockService
    {
        /// <summary>
        /// Create a Buy order with the given informations in buyOrderRequest
        /// </summary>
        /// <param name="buyOrderRequest"></param>
        /// <returns>Returns the created object</returns>
        Task<BuyOrderResponse> CreateBuyOrder(BuyOrderRequest? buyOrderRequest);
        /// <summary>
        /// Create a Sell order with the given informations in sellOrderRequest
        /// </summary>
        /// <param name="sellOrderRequest"></param>
        /// <returns>Returns the created object</returns>
        Task<SellOrderResponse> CreateSellOrder(SellOrderRequest? sellOrderRequest);
        /// <summary>
        /// Marks a sell order as cancelled
        /// </summary>
        /// <param name="sellOrderId">The sell order to cancel</param>
        /// <returns>A boolean indicating if the sell order is successfully cancelled.</returns>
        Task<bool> CancelSellOrder(Guid sellOrderId);
        /// <summary>
        /// Get the list of buy orders saved in the database
        /// </summary>
        /// <returns></returns>
        Task<List<BuyOrderResponse>> GetAllBuyOrders();
        /// <summary>
        /// Get the list of sell orders saved in the database
        /// </summary>
        /// <returns></returns>
        Task<List<SellOrderResponse>> GetAllSellOrders();
    }
}
