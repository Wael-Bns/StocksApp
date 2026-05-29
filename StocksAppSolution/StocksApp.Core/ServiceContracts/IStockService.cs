using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.SellOrderDTO;

namespace StocksApp.Core.ServiceContracts
{
    public interface IStockService
    {
        /// <summary>
        /// Create a Buy order with the given informations in buyOrderRequest
        /// </summary>
        /// <param name="buyOrderRequest"></param>
        /// <returns>Returns the created object</returns>
        Task<BuyOrderResponse> CreateBuyOrder(BuyOrderAddRequest? buyOrderRequest);
        /// <summary>
        /// Create a Sell order with the given informations in sellOrderRequest
        /// </summary>
        /// <param name="sellOrderRequest"></param>
        /// <returns>Returns the created object</returns>
        Task<SellOrderResponse> CreateSellOrder(SellOrderAddRequest? sellOrderRequest);
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
