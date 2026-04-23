using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.DTO;
using StocksApp.Core.Helpers;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class StockService : IStockService
    {
        private readonly IOrderRepository _orderRepository;

        public StockService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public Task<bool> CancelSellOrder(Guid sellOrderId)
        {
            throw new NotImplementedException();
        }

        public async Task<BuyOrderResponse> CreateBuyOrder(BuyOrderRequest? buyOrderRequest)
        {
            if(buyOrderRequest == null)
            {
                throw new ArgumentNullException(nameof(buyOrderRequest));
            }
            // Validate the model 
            ValidationHelper.ModelValidation(buyOrderRequest);

            var buyOrder = buyOrderRequest.ToBuyOrder();
            
            BuyOrder createdBuyOrder = await _orderRepository.AddBuyOrderAsync(buyOrder);

            return createdBuyOrder.ToBuyOrderResponse();
        }

        public async Task<SellOrderResponse> CreateSellOrder(SellOrderRequest? sellOrderRequest)
        {
            if (sellOrderRequest == null)
            {
                throw new ArgumentNullException(nameof(sellOrderRequest));
            }
            // Validate the model 
            ValidationHelper.ModelValidation(sellOrderRequest);

            var sellOrder = sellOrderRequest.ToSellOrder();

            SellOrder createdSellOrder = await _orderRepository.AddSellOrderAsync(sellOrder);

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
