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
        /// <returns>Returns the created object</returns>
        Task<BuyOrderResponse> CreateBuyOrder(BuyOrderRequest? buyOrderRequest);
        /// <summary>
        /// Create a Sell order with the given informations in sellOrderRequest
        /// </summary>
        /// <param name="sellOrderRequest"></param>
        /// <returns>Returns the created object</returns>
        Task<SellOrderResponse> CreateSellOrder(SellOrderRequest? sellOrderRequest);
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
        /// <summary>
        /// Returns the stock informations based on the provided stock symbol .
        /// </summary>
        /// <param name="stockSymbol">Symbol based on which informations are fetched .</param>
        /// <returns>Stock informations</returns>
        Task<StockInformations> GetStockInformations(string stockSymbol);
    }
}
