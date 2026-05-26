using StocksApp.Core.DTO.SellOrderDTO;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// This class is responsible for managing bu and sell orders (CRUD operations and websocket subscriptions) .  
    /// </summary>
    public interface IOrdersManagementService
    {
        Task<SellOrderResponse> AddSellOrder(SellOrderAddRequest sellOrderAddRequest);
    }
}
