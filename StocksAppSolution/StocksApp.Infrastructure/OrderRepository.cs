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
