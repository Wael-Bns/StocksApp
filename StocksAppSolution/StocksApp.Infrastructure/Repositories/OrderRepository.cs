using Microsoft.EntityFrameworkCore;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.Domain.Specifications;
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
                    .Where(order => order.Status == SellOrderStatus.Pending  
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
                    order.Status = SellOrderStatus.Executed;
                    order.User.CashBalance += (order.Price * order.Quantity);
                }

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return ordersToExecute;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // Re-throw the exception after rolling back
            }
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
        public async Task<List<SellOrder>> GetSellOrdersBySpecification(ISpecification<SellOrder> specification)
        {
            return await _dbContext.SellOrders.Where(specification.Criteria).ToListAsync();
        }
    }
}
