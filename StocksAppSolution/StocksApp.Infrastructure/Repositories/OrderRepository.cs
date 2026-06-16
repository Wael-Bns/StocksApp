using Microsoft.EntityFrameworkCore;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.Domain.Specifications;

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

        public async Task<List<SellOrder>> GetSellOrdersBySpecification(ISpecification<SellOrder> specification)
        {
            return await _dbContext.SellOrders.Where(specification.Criteria).ToListAsync();
        }
    }
}
