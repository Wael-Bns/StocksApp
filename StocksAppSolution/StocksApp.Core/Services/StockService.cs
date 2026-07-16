using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;
using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.Exceptions;
using StocksApp.Core.Helpers;
using StocksApp.Core.HttpClientAbstractions;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class StockService : IStockService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IFinnHubHttpClient _finnhubHttpClient;

        public StockService(IOrderRepository orderRepository, IFinnHubHttpClient finnHubHttpClient)
        {
            _orderRepository = orderRepository;
            _finnhubHttpClient = finnHubHttpClient;
        }
        public async Task<BuyOrderResponse> CreateBuyOrder(BuyOrderAddRequest? buyOrderRequest, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(buyOrderRequest);

            ValidationHelper.ModelValidation(buyOrderRequest);

            var buyOrder = buyOrderRequest.ToBuyOrder();
            buyOrder.UserId = userId;
            
            BuyOrder createdBuyOrder = await _orderRepository.AddBuyOrderAsync(buyOrder);

            return createdBuyOrder.ToBuyOrderResponse();
        }

        public async Task<SellOrderResponse> CreateSellOrder(SellOrderAddRequest? sellOrderRequest, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(sellOrderRequest);

            ValidationHelper.ModelValidation(sellOrderRequest);

            var sellOrder = sellOrderRequest.ToSellOrder();
            sellOrder.UserId = userId;

            SellOrder createdSellOrder = await _orderRepository.AddSellOrderAsync(sellOrder);

            return createdSellOrder.ToSellOrderResponse();
        }

        public async Task<List<BuyOrderResponse>> GetBuyOrdersByUser(Guid userId)
        {
            var specification = new BuyOrdersByUserSpecification(userId);
            List<BuyOrder> orderRequests = await _orderRepository.GetBuyOrdersBySpecification(specification);
            return orderRequests.Select(o => o.ToBuyOrderResponse()).ToList();
        }

        public async Task<List<SellOrderResponse>> GetSellOrdersByUser(Guid userId)
        {
            var specification = new SellOrderByUserSpecification(userId);
            List<SellOrder> sellOrderRequests = await _orderRepository.GetSellOrdersBySpecificationAsNoTracking(specification);
            return sellOrderRequests.Select(o => o.ToSellOrderResponse()).ToList();
        }

        public async Task<StockInformations> GetStockInformations(string stockSymbol)
        {
            var stockQuoteTask = _finnhubHttpClient.GetStockQuote(stockSymbol);
            var stockProfileTask = _finnhubHttpClient.GetCompanyProfile(stockSymbol);

            var stockQuote = await stockQuoteTask;
            var stockProfile = await stockProfileTask;

            if (stockQuote == null || stockQuote.CurrentPrice == 0 || stockProfile == null)
            {
                throw new StockNotFoundException($"Stock with symbol '{stockSymbol}' was not found.");
            }

            return new StockInformations
            {
                StockSymbol = stockProfile.Ticker,
                StockName = stockProfile.Name,
                Currency = stockProfile.Currency,
                Exchange = stockProfile.Exchange,
                WebUrl = stockProfile.WebUrl,
                Industry = stockProfile.FinnhubIndustry,
                PricePerShare = stockQuote.CurrentPrice,
                Logo = stockProfile.Logo
            };
        }
    }
}
