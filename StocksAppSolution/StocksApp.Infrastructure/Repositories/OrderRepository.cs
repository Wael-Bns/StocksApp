using Microsoft.EntityFrameworkCore;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.Enums;

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
        public async Task<List<BuyOrder>> GetAllBuyOrdersAsync()
        {
            return await _dbContext.BuyOrders.ToListAsync();
        }

        public async Task<List<SellOrder>> GetAllSellOrdersAsync()
        {
            return await _dbContext.SellOrders.ToListAsync();
        }

        public async Task<BuyOrder?> GetBuyOrder(Guid orderID)
        {
            return await _dbContext.BuyOrders.FirstOrDefaultAsync(order => order.BuyOrderID == orderID);
        }

        public async Task<SellOrder?> GetSellOrder(Guid orderID)
        {
            return await _dbContext.SellOrders.FirstOrDefaultAsync(order => order.SellOrderID == orderID);
        }
        public async Task<bool> UpdateSellOrder(SellOrder sellOrder)
        {
            _dbContext.SellOrders.Update(sellOrder);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task ExecuteSellOrderAsync(SellOrder sellOrder, ApplicationUser user, double proceeds)
        {
            // Begin a transaction (or rely on SaveChanges for atomicity since both share the same context)
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            { 
                sellOrder.SellOrderStatus = SellOrderStatusEnum.Executed.ToString();
                user.CashBalance += proceeds;

                // Commits both updates in a single atomic database trip
                await _dbContext.SaveChangesAsync(); 
                
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // Re-throw to be handled and logged by the caller
            }
        }
    }
}
