using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.Enums;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class TradeExecutionService : ITradeExecutionService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<TradeExecutionService> _logger;

        public TradeExecutionService(IUserRepository userRepository, IOrderRepository orderRepository, ILogger<TradeExecutionService> logger)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task FinalizeSellOrderAsync(SellOrder incomingOrder)
        {
            var sellOrderInDb = await _orderRepository.GetSellOrder(incomingOrder.SellOrderID);
            if (sellOrderInDb == null)
            {
                _logger.LogWarning("Sell order {ID} not found in DB.", incomingOrder.SellOrderID);
                return;
            }

            var user = await _userRepository.GetUserByUserId(sellOrderInDb.UserId);
            if (user == null)
            {
                _logger.LogWarning("User {ID} not found.", sellOrderInDb.UserId);
                return;
            }

            // Execute Business Logic
            double totalProceeds = incomingOrder.Quantity * incomingOrder.Price;
            await _orderRepository.ExecuteSellOrderAsync(sellOrderInDb, user, totalProceeds);
        }
    }
}