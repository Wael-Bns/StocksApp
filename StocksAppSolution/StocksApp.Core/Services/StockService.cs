using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.Helpers;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class StockService : IStockService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ISubscriptionsManager _subscriptionsManager;

        public StockService(IOrderRepository orderRepository, ISubscriptionsManager subscriptionsManager)
        {
            _orderRepository = orderRepository;
            _subscriptionsManager = subscriptionsManager;
        }
        public async Task<BuyOrderResponse> CreateBuyOrder(BuyOrderAddRequest? buyOrderAddRequest)
        {
            if(buyOrderAddRequest == null)
            {
                throw new ArgumentNullException(nameof(buyOrderAddRequest));
            }
            // Validate the model 
            ValidationHelper.ModelValidation(buyOrderAddRequest);

            var buyOrder = buyOrderAddRequest.ToBuyOrder();
            
            BuyOrder createdBuyOrder = await _orderRepository.AddBuyOrderAsync(buyOrder);

            return createdBuyOrder.ToBuyOrderResponse();
        }

        public async Task<SellOrderResponse> CreateSellOrder(SellOrderAddRequest? sellOrderAddRequest)
        {
            if (sellOrderAddRequest == null)
            {
                throw new ArgumentNullException(nameof(sellOrderAddRequest));
            }
            // Validate the model 
            ValidationHelper.ModelValidation(sellOrderAddRequest);

            var sellOrder = sellOrderAddRequest.ToSellOrder();

            SellOrder createdSellOrder = await _orderRepository.AddSellOrderAsync(sellOrder);

            await _subscriptionsManager.AddStockSymbol(sellOrderAddRequest.StockSymbol!);

            return createdSellOrder.ToSellOrderResponse();
        }

        public async Task<List<BuyOrderResponse>> GetAllBuyOrders()
        {
            List<BuyOrder> orderRequests = await _orderRepository.GetAllBuyOrdersAsync();
            return orderRequests.Select(o => o.ToBuyOrderResponse()).ToList();
        }

        public async Task<List<SellOrderResponse>> GetAllSellOrders()
        {
            List<SellOrder> sellOrderRequests = await _orderRepository.GetAllSellOrdersAsync();
            return sellOrderRequests.Select(o => o.ToSellOrderResponse()).ToList();
        }
    }
}
