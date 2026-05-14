using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.Helpers;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly ISubscriptionsManager _subscriptionsManager;
        private readonly IOrderRepository _orderRepository;
        public OrdersService(ISubscriptionsManager subscriptionsManager, IOrderRepository orderRepository)
        {
            _subscriptionsManager = subscriptionsManager;
            _orderRepository = orderRepository;
        }
        public async Task<SellOrderResponse> AddSellOrder(SellOrderAddRequest sellOrderAddRequest)
        {
            if(sellOrderAddRequest == null)
            {
                throw new ArgumentNullException(nameof(sellOrderAddRequest));
            }
            // Validate the model
            ValidationHelper.ModelValidation(sellOrderAddRequest);
            
            SellOrder addedSellOrder = await _orderRepository.AddSellOrderAsync(sellOrderAddRequest.ToSellOrder());
            
            await _subscriptionsManager.AddStockSymbol(sellOrderAddRequest.StockSymbol!);
            
            return addedSellOrder.ToSellOrderResponse();
        }
    }
}
