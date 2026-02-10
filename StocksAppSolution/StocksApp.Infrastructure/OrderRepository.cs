using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;

namespace StocksApp.Infrastructure
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
