using Microsoft.Extensions.Options;
using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.Exceptions;
using StocksApp.Core.Helpers;
using StocksApp.Core.HttpClientAbstractions;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Events;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;

namespace StocksApp.Core.Services
{
    public class StockService : IStockService
    {
        private readonly IGenericRepository<BuyOrder> _buyOrderRepository;
        private readonly IGenericRepository<SellOrder> _sellOrderRepository;
        private readonly IGenericRepository<Outbox> _outboxRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFinnHubHttpClient _finnhubHttpClient;
        private readonly ICandleCache _candleCache;
        private readonly IOptions<OrderMatchingOptions> _matchingOptions;

        public StockService(
            IGenericRepository<BuyOrder> buyOrderRepository,
            IGenericRepository<SellOrder> sellOrderRepository,
            IGenericRepository<Outbox> outboxRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IFinnHubHttpClient finnHubHttpClient,
            ICandleCache candleCache,
            IOptions<OrderMatchingOptions> matchingOptions)
        {
            _buyOrderRepository = buyOrderRepository;
            _sellOrderRepository = sellOrderRepository;
            _outboxRepository = outboxRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _finnhubHttpClient = finnHubHttpClient;
            _candleCache = candleCache;
            _matchingOptions = matchingOptions;
        }

        public async Task<BuyOrderResponse> CreateBuyOrder(BuyOrderAddRequest? buyOrderRequest, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(buyOrderRequest);
            ValidationHelper.ModelValidation(buyOrderRequest);

            var buyOrder = buyOrderRequest.ToBuyOrder();
            buyOrder.UserId = userId;

            var createdBuyOrder = await _buyOrderRepository.AddAsync(buyOrder);
            return createdBuyOrder.ToBuyOrderResponse();
        }

        public async Task<SellOrderResponse> CreateSellOrder(SellOrderAddRequest? sellOrderRequest, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(sellOrderRequest);
        ValidationHelper.ModelValidation(sellOrderRequest);

        var options = _matchingOptions.Value;
        var sellOrder = SellOrder.Create(
            userId, sellOrderRequest.StockSymbol!, sellOrderRequest.StockName,
            sellOrderRequest.Price, sellOrderRequest.Quantity, DateTime.UtcNow, options.MatchBucketSize);

        var latest = await _candleCache.GetLatestPriceAsync(sellOrder.StockSymbol, CancellationToken.None);
        var marketable = latest is { } p
            && (DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(p.Timestamp)) <= options.MaxImmediateFillPriceAge
            && (double)p.Price >= sellOrder.Price;

        await _unitOfWork.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var createdSellOrder = await _sellOrderRepository.AddAsync(sellOrder);

            if (marketable)
            {
                var user = await _userRepository.GetByIdAsync(userId, CancellationToken.None)
                    ?? throw new InvalidOperationException($"User {userId} not found.");
                createdSellOrder.MarkExecuted(user);

                var outboxEvent = SellOrderExecuted.From(createdSellOrder).ToOutbox();
                await _outboxRepository.AddAsync(outboxEvent);
            }

            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            await _unitOfWork.CommitTransactionAsync(CancellationToken.None);

            return createdSellOrder.ToSellOrderResponse();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

        public async Task<List<BuyOrderResponse>> GetBuyOrdersByUser(Guid userId)
        {
            var spec = new BuyOrdersByUserSpecification(userId);
            var orders = await _buyOrderRepository.ListAsync(spec);
            return orders.Select(o => o.ToBuyOrderResponse()).ToList();
        }

        public async Task<List<SellOrderResponse>> GetSellOrdersByUser(Guid userId)
        {
            var spec = new SellOrderByUserSpecification(userId);
            var orders = await _sellOrderRepository.ListAsync(spec);
            return orders.Select(o => o.ToSellOrderResponse()).ToList();
        }

        public async Task<StockInformations> GetStockInformations(string stockSymbol)
        {
            var stockQuoteTask = _finnhubHttpClient.GetStockQuote(stockSymbol);
            var stockProfileTask = _finnhubHttpClient.GetCompanyProfile(stockSymbol);
            var stockQuote = await stockQuoteTask;
            var stockProfile = await stockProfileTask;

            if (stockQuote == null || stockQuote.CurrentPrice == 0 || stockProfile == null)
                throw new StockNotFoundException($"Stock with symbol '{stockSymbol}' was not found.");

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