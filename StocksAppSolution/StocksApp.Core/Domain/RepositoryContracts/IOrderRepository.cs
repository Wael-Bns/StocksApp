using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.Specifications;

namespace StocksApp.Core.Domain.RepositoryContracts
{
    /// <summary>
    /// A Repository responsible for handling data access related to Buy and Sell Orders. 
    /// </summary>
    public interface IOrderRepository
    {
        Task<BuyOrder?> GetBuyOrder(Guid buyOrderId);
        Task<SellOrder?> GetSellOrder(Guid sellOrderId);
        Task<BuyOrder> AddBuyOrderAsync(BuyOrder buyOrder);
        Task<SellOrder> AddSellOrderAsync(SellOrder sellOrder);
        Task<List<SellOrder>> GetSellOrdersBySpecification(ISpecification<SellOrder> specification);
        Task<List<BuyOrder>> GetBuyOrdersBySpecification(ISpecification<BuyOrder> specification);
    }
}
