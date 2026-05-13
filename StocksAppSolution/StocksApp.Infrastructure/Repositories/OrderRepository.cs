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

        public async Task<IEnumerable<SellOrder>?> ExecuteSellOrders(string stockSymbol, double marketPrice)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                IEnumerable<SellOrder> ordersToExecute = await _dbContext.SellOrders
                    .Where(order => order.Status == (int)SellOrderStatus.Pending  
                           && order.Price <= marketPrice
                           && order.StockSymbol == stockSymbol)
                    .Include(order => order.User)
                    .ToListAsync();

                if (!ordersToExecute.Any())
                {
                    return null;
                }

                foreach (var order in ordersToExecute)
                {
                    order.Status = (int)SellOrderStatus.Executed;
                    order.User.CashBalance += (order.Price * order.Quantity);
                }

                // 3. Save changes in a single batch
                await _dbContext.SaveChangesAsync();

                // 4. Commit transaction
                await transaction.CommitAsync();

                return ordersToExecute;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // Re-throw the exception after rolling back
            }
        }

        public async Task<List<BuyOrder>> GetAllBuyOrdersAsync()
        {
            return await _dbContext.BuyOrders.ToListAsync();
        }

        public async Task<List<SellOrder>> GetAllSellOrdersAsync()
        {
            return await _dbContext.SellOrders.ToListAsync();
        }

        public Task<BuyOrder?> GetBuyOrder(Guid orderID)
        {
            return _dbContext.BuyOrders.FirstOrDefaultAsync(order => order.BuyOrderID == orderID);
        }

        public Task<SellOrder?> GetSellOrder(Guid orderID)
        {
            return _dbContext.SellOrders.FirstOrDefaultAsync(order => order.SellOrderID == orderID);
        }
    }
}
