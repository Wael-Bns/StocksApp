using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.Domain.RepositoryContracts
{
    /// <summary>
    /// A Repository that contains database methods that need to be implemented by a child class
    /// </summary>
    public interface IOrderRepository
    {
        Task<List<BuyOrder>> GetAllBuyOrdersAsync();
        Task<List<SellOrder>> GetAllSellOrdersAsync();
        Task<BuyOrder?> GetBuyOrder(Guid buyOrderId);
        Task<SellOrder?> GetSellOrder(Guid sellOrderId);
        Task<BuyOrder> AddBuyOrderAsync(BuyOrder buyOrder);
        Task<SellOrder> AddSellOrderAsync(SellOrder sellOrder);
    }
}
