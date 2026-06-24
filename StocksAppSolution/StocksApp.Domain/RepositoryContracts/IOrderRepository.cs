using StocksApp.Domain.Entities;
using StocksApp.Domain.Specifications;

namespace StocksApp.Domain.RepositoryContracts
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
        Task<List<string>> GetPendingSellOrderSymbols();
        Task<List<SellOrder>> GetSellOrdersBySpecificationAsNoTracking(ISpecification<SellOrder> specification);
        Task<List<BuyOrder>> GetBuyOrdersBySpecification(ISpecification<BuyOrder> specification);
        Task<IEnumerable<SellOrder>?> ExecuteSellOrders(string stockSymbol, double marketPrice);
        Task<List<SellOrder>> GetSellOrdersByIds(List<Guid> sellOrderIds);
    }
}
