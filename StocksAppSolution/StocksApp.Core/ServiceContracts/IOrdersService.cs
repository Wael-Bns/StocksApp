using StocksApp.Core.DTO.SellOrderDTO;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// This class is responsible for handling all the business logic related to Orders (Buy and Sell).  
    /// </summary>
    public interface IOrdersService
    {
        Task<SellOrderResponse> AddSellOrder(SellOrderAddRequest sellOrderAddRequest);
    }
}
