using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
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
        private readonly ISubscriptionsManager _subscriptionsManager;
        private readonly IFinnHubHttpClient _finnhubHttpClient;

        public StockService(IOrderRepository orderRepository, IFinnHubHttpClient finnHubHttpClient, ISubscriptionsManager subscriptionsManager)
        {
            _orderRepository = orderRepository;
            _subscriptionsManager = subscriptionsManager;
            _finnhubHttpClient = finnHubHttpClient;
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
