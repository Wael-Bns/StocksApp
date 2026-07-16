using Microsoft.EntityFrameworkCore;
using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;
using StocksApp.Domain.Enums;

namespace StocksApp.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public OrderRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BuyOrder> AddBuyOrderAsync(BuyOrder buyOrder)
        {
            await _dbContext.BuyOrders.AddAsync(buyOrder);
            await _dbContext.SaveChangesAsync();
            return buyOrder;
        }

        public async Task<SellOrder> AddSellOrderAsync(SellOrder sellOrder)
        {
            await _dbContext.SellOrders.AddAsync(sellOrder);
            await _dbContext.SaveChangesAsync();
            return sellOrder;
        }
       public async Task<BuyOrder?> GetBuyOrder(Guid orderID)
        {
            return await _dbContext.BuyOrders.FirstOrDefaultAsync(order => order.BuyOrderID == orderID);
        }

        public async Task<List<BuyOrder>> GetBuyOrdersBySpecification(ISpecification<BuyOrder> specification)
        {
            return await _dbContext.BuyOrders.Where(specification.Criteria).ToListAsync();
        }

        public async Task<SellOrder?> GetSellOrder(Guid orderID)
        {
            return await _dbContext.SellOrders.FirstOrDefaultAsync(order => order.SellOrderID == orderID);
        }

        public async Task<List<string>> GetPendingSellOrderSymbols()
        {
            var symbols = await _dbContext.SellOrders.
                Where(order => order.Status == SellOrderStatus.Pending)
                .AsNoTracking()
                .Select(order => order.StockSymbol!)
                .Distinct()
                .ToListAsync();
            return symbols;
        }
        public async Task<List<SellOrder>> GetSellOrdersBySpecificationAsNoTracking(ISpecification<SellOrder> specification)
        {
            return await _dbContext.SellOrders
                .AsNoTracking()
                .Where(specification.Criteria)
                .ToListAsync();
        }

        public async Task<List<SellOrder>> GetSellOrdersByIds(List<Guid> sellOrderIds)
        {
            List<SellOrder> sellOrders = await _dbContext.SellOrders
                .Where(order => sellOrderIds.Contains(order.SellOrderID))
                .Include(o => o.User)
                .ToListAsync();
            return sellOrders;
        }
    }
}
