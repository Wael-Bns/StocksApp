using Microsoft.Extensions.Logging;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class OrdersExecutor : IOrdersExecutor
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrdersExecutor> _logger;
        public OrdersExecutor(IOrderRepository orderRepository, ILogger<OrdersExecutor> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }
        public async Task<List<SellOrderResponse>?> ExecuteSellOrders(string stockSymbol, double marketPrice)
        {            
            IEnumerable<SellOrder>? executedSellOrders = await _orderRepository.ExecuteSellOrders(stockSymbol,marketPrice);
            if(executedSellOrders == null)
            {
                _logger.LogInformation("No sell orders were executed.");
                return null;
            }
            List<SellOrderResponse> sellOrderResponses = executedSellOrders.Select(sellorder => sellorder.ToSellOrderResponse()).ToList();
            
            _logger.LogInformation("Executed {Count} sell orders.", sellOrderResponses.Count);

            return sellOrderResponses;

        }
    }
}
