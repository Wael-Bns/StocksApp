// StocksApp.Test.Core/StockServiceTest.cs
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.CandleDTO;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.Exceptions;
using StocksApp.Core.HttpClientAbstractions;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using StocksApp.Domain.Constants;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.Core
{
    public class StockServiceTest
    {
        private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        private readonly Mock<IGenericRepository<BuyOrder>> _buyOrderRepositoryMock;
        private readonly Mock<IGenericRepository<SellOrder>> _sellOrderRepositoryMock;
        private readonly Mock<IGenericRepository<Outbox>> _outboxRepositoryMock;
        private readonly Mock<ISellOrderMatchRepository> _matchRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IFinnHubHttpClient> _finnHubHttpClientMock;
        private readonly Mock<ICandleCache> _candleCacheMock;
        private readonly OrderMatchingOptions _matchingOptions;
        private readonly StockService _stockService;

        private readonly BuyOrderAddRequest _buyOrderRequest = new()
        {
            StockName = "APPLE INC",
            StockSymbol = "AAPL",
            Quantity = 250,
            DateAndTimeOfOrder = DateTime.Now,
            Price = 100,
            UserId = Guid.Parse("EB608896-7E47-44A6-9395-5D4EEE695044")
        };

        public StockServiceTest()
        {
            _buyOrderRepositoryMock = new Mock<IGenericRepository<BuyOrder>>();
            _sellOrderRepositoryMock = new Mock<IGenericRepository<SellOrder>>();
            _outboxRepositoryMock = new Mock<IGenericRepository<Outbox>>();
            _matchRepositoryMock = new Mock<ISellOrderMatchRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _finnHubHttpClientMock = new Mock<IFinnHubHttpClient>();
            _candleCacheMock = new Mock<ICandleCache>();

            _matchingOptions = new OrderMatchingOptions
            {
                MatchBucketSize = TimeSpan.FromSeconds(5),
                MaxImmediateFillPriceAge = TimeSpan.FromSeconds(10),
                MatcherGracePeriod = TimeSpan.FromSeconds(2)
            };

            // mirrors GenericRepository<T>.AddAsync: returns the same tracked entity passed in
            _sellOrderRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<SellOrder>()))
                .ReturnsAsync((SellOrder order) => order);

            _stockService = new StockService(
                _buyOrderRepositoryMock.Object,
                _sellOrderRepositoryMock.Object,
                _outboxRepositoryMock.Object,
                _matchRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _finnHubHttpClientMock.Object,
                _candleCacheMock.Object,
                Options.Create(_matchingOptions));
        }

        #region Helpers

        private void MockAddBuyOrder(BuyOrder buyOrder) =>
            _buyOrderRepositoryMock.Setup(r => r.AddAsync(It.IsAny<BuyOrder>())).ReturnsAsync(buyOrder);

        private void MockGetBuyOrdersBySpecification(List<BuyOrder> buyOrders) =>
            _buyOrderRepositoryMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<BuyOrder>>())).ReturnsAsync(buyOrders);

        private void MockGetSellOrdersBySpecification(List<SellOrder> sellOrders) =>
            _sellOrderRepositoryMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<SellOrder>>())).ReturnsAsync(sellOrders);

        private void MockGetStockQuote(StockQuoteDTO? stockQuote) =>
            _finnHubHttpClientMock.Setup(c => c.GetStockQuote(It.IsAny<string>())).ReturnsAsync(stockQuote);

        private void MockGetCompanyProfile(CompanyProfileDTO? companyProfile) =>
            _finnHubHttpClientMock.Setup(c => c.GetCompanyProfile(It.IsAny<string>())).ReturnsAsync(companyProfile);

        private void MockLatestPrice(string symbol, LatestPriceSnapshot? snapshot) =>
            _candleCacheMock.Setup(c => c.GetLatestPriceAsync(symbol, It.IsAny<CancellationToken>()))
                .ReturnsAsync(snapshot);

        private async Task AssertBuyOrderRejected()
        {
            Func<Task> act = () => _stockService.CreateBuyOrder(_buyOrderRequest, TestUserId);

            await act.Should().ThrowAsync<InvalidPropertyException>();
        }

        #endregion

        #region Create Buy Order

        [Fact]
        public async Task CreateBuyOrder_NullRequest_ThrowsArgumentNullException()
        {
            Func<Task> act = () => _stockService.CreateBuyOrder(null, TestUserId);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateBuyOrder_QuantityLessThanMinimum_ThrowsInvalidPropertyException()
        {
            _buyOrderRequest.Quantity = 0;

            await AssertBuyOrderRejected();
        }

        [Fact]
        public async Task CreateBuyOrder_QuantityMoreThanMaximum_ThrowsInvalidPropertyException()
        {
            _buyOrderRequest.Quantity = 10001;

            await AssertBuyOrderRejected();
        }

        [Fact]
        public async Task CreateBuyOrder_PriceLessThanMinimum_ThrowsInvalidPropertyException()
        {
            _buyOrderRequest.Price = 0;

            await AssertBuyOrderRejected();
        }

        [Fact]
        public async Task CreateBuyOrder_PriceMoreThanMaximum_ThrowsInvalidPropertyException()
        {
            _buyOrderRequest.Price = 10001;

            await AssertBuyOrderRejected();
        }

        [Fact]
        public async Task CreateBuyOrder_NullStockSymbol_ThrowsInvalidPropertyException()
        {
            _buyOrderRequest.StockSymbol = null;

            await AssertBuyOrderRejected();
        }

        [Fact]
        public async Task CreateBuyOrder_DateLessThanMinimum_ThrowsInvalidPropertyException()
        {
            _buyOrderRequest.DateAndTimeOfOrder = Convert.ToDateTime("1999-12-31");

            await AssertBuyOrderRejected();
        }

        [Fact]
        public async Task CreateBuyOrder_ValidRequest_ReturnsCreatedOrder()
        {
            // Arrange
            var buyOrder = _buyOrderRequest.ToBuyOrder();
            buyOrder.UserId = TestUserId;
            var expected = buyOrder.ToBuyOrderResponse();
            MockAddBuyOrder(buyOrder);

            // Act
            var actual = await _stockService.CreateBuyOrder(_buyOrderRequest, TestUserId);

            // Assert
            actual.Should().BeEquivalentTo(expected);
            actual.BuyOrderID.Should().NotBeEmpty();
        }

        #endregion

        #region Create Sell Order — Validation

        [Fact]
        public async Task CreateSellOrder_NullRequest_ThrowsArgumentNullException()
        {
            Func<Task> act = () => _stockService.CreateSellOrder(null, TestUserId);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        #endregion

        #region Create Sell Order — Immediate-Fill Decision

        [Fact]
        public async Task CreateSellOrder_NoCachedPrice_CreatesPendingOrderWithoutExecuting()
        {
            // Arrange
            var request = new SellOrderAddRequestBuilder().WithPrice(100).Build();
            MockLatestPrice(request.StockSymbol!, null);

            // Act
            var response = await _stockService.CreateSellOrder(request, TestUserId);

            // Assert
            response.Status.Should().Be(SellOrderStatus.Pending);
            _matchRepositoryMock.Verify(
                m => m.CreditCashAsync(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _outboxRepositoryMock.Verify(o => o.AddAsync(It.IsAny<Outbox>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateSellOrder_CachedPriceOlderThanMaxImmediateFillAge_DoesNotExecute()
        {
            // Arrange
            var request = new SellOrderAddRequestBuilder().WithPrice(100).Build();
            var stale = new LatestPriceSnapshotBuilder()
                .WithSymbol(request.StockSymbol!)
                .WithPrice(150m)
                .WithTimestamp(DateTimeOffset.UtcNow - _matchingOptions.MaxImmediateFillPriceAge - TimeSpan.FromSeconds(1))
                .Build();
            MockLatestPrice(request.StockSymbol!, stale);

            // Act
            var response = await _stockService.CreateSellOrder(request, TestUserId);

            // Assert
            response.Status.Should().Be(SellOrderStatus.Pending);
            _matchRepositoryMock.Verify(
                m => m.CreditCashAsync(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateSellOrder_FreshCachedPriceBelowLimit_DoesNotExecute()
        {
            // Arrange
            var request = new SellOrderAddRequestBuilder().WithPrice(100).Build();
            var fresh = new LatestPriceSnapshotBuilder()
                .WithSymbol(request.StockSymbol!)
                .WithPrice(99m)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .Build();
            MockLatestPrice(request.StockSymbol!, fresh);

            // Act
            var response = await _stockService.CreateSellOrder(request, TestUserId);

            // Assert
            response.Status.Should().Be(SellOrderStatus.Pending);
        }

        [Fact]
        public async Task CreateSellOrder_FreshCachedPriceAtOrAboveLimit_ExecutesImmediatelyAndCreditsCash()
        {
            // Arrange
            var request = new SellOrderAddRequestBuilder().WithPrice(100).WithQuantity(10).Build();
            var fresh = new LatestPriceSnapshotBuilder()
                .WithSymbol(request.StockSymbol!)
                .WithPrice(100m)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .Build();
            MockLatestPrice(request.StockSymbol!, fresh);

            // Act
            var response = await _stockService.CreateSellOrder(request, TestUserId);

            // Assert
            response.Status.Should().Be(SellOrderStatus.Executed);
            _matchRepositoryMock.Verify(
                m => m.CreditCashAsync(TestUserId, 100 * 10, It.IsAny<CancellationToken>()), Times.Once);
            _outboxRepositoryMock.Verify(
                o => o.AddAsync(It.Is<Outbox>(e => e.EventName == EventNames.SellOrderExecuted)), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Create Sell Order — Failure Handling

        [Fact]
        public async Task CreateSellOrder_RepositoryThrows_RollsBackAndPreservesOriginalExceptionType()
        {
            // Arrange
            var request = new SellOrderAddRequestBuilder().Build();
            _candleCacheMock
                .Setup(c => c.GetLatestPriceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((LatestPriceSnapshot?)null);
            _sellOrderRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<SellOrder>()))
                .ThrowsAsync(new InvalidOperationException("db unavailable"));

            // Act
            Func<Task> act = () => _stockService.CreateSellOrder(request, TestUserId);

            // Assert — the real exception type must survive, not a wrapped generic Exception
            await act.Should().ThrowAsync<InvalidOperationException>();
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Cancel Sell Order

        [Fact]
        public async Task CancelSellOrder_OrderNotFoundForUser_ReturnsFalseWithoutCallingMatchRepository()
        {
            // Arrange
            _sellOrderRepositoryMock
                .Setup(r => r.GetAsync(It.IsAny<ISpecification<SellOrder>>()))
                .ReturnsAsync((SellOrder?)null);

            // Act
            var result = await _stockService.CancelSellOrder(Guid.NewGuid(), TestUserId);

            // Assert
            result.Should().BeFalse();
            _matchRepositoryMock.Verify(
                m => m.TryCancelAsync(It.IsAny<Guid>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CancelSellOrder_OwnedOrder_ReturnsMatchRepositoryResult(bool matchRepositoryResult)
        {
            // Arrange
            var order = CreateSellOrder();
            _sellOrderRepositoryMock
                .Setup(r => r.GetAsync(It.IsAny<ISpecification<SellOrder>>()))
                .ReturnsAsync(order);
            _matchRepositoryMock
                .Setup(m => m.TryCancelAsync(order.SellOrderID, _matchingOptions.MatcherGracePeriod, It.IsAny<CancellationToken>()))
                .ReturnsAsync(matchRepositoryResult);

            // Act
            var result = await _stockService.CancelSellOrder(order.SellOrderID, TestUserId);

            // Assert
            result.Should().Be(matchRepositoryResult);
        }

        #endregion

        #region Get Orders By User

        [Fact]
        public async Task GetBuyOrdersByUser_NoOrders_ReturnsEmptyList()
        {
            // Arrange
            MockGetBuyOrdersBySpecification([]);

            // Act
            var actual = await _stockService.GetBuyOrdersByUser(TestUserId);

            // Assert
            actual.Should().BeEmpty();
        }

        [Fact]
        public async Task GetBuyOrdersByUser_WithOrders_ReturnsMappedResponses()
        {
            // Arrange
            List<BuyOrder> buyOrders = [_buyOrderRequest.ToBuyOrder()];
            MockGetBuyOrdersBySpecification(buyOrders);
            var expected = buyOrders.Select(o => o.ToBuyOrderResponse()).ToList();

            // Act
            var actual = await _stockService.GetBuyOrdersByUser(TestUserId);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetSellOrdersByUser_NoOrders_ReturnsEmptyList()
        {
            // Arrange
            MockGetSellOrdersBySpecification([]);

            // Act
            var actual = await _stockService.GetSellOrdersByUser(TestUserId);

            // Assert
            actual.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSellOrdersByUser_WithOrders_ReturnsMappedResponses()
        {
            // Arrange
            List<SellOrder> sellOrders = [CreateSellOrder()];
            MockGetSellOrdersBySpecification(sellOrders);
            var expected = sellOrders.Select(o => o.ToSellOrderResponse()).ToList();

            // Act
            var actual = await _stockService.GetSellOrdersByUser(TestUserId);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        #endregion

        #region Get Stock Informations

        [Fact]
        public async Task GetStockInformations_ValidSymbol_ReturnsStockInformations()
        {
            // Arrange
            const string stockSymbol = "AAPL";
            var stockQuote = new StockQuoteDTO { CurrentPrice = 192.53m };
            var companyProfile = new CompanyProfileDTO
            {
                Ticker = stockSymbol,
                Name = "Apple Inc",
                Currency = "USD",
                Exchange = "NASDAQ NMS - GLOBAL MARKET",
                WebUrl = "https://www.apple.com/",
                FinnhubIndustry = "Technology",
                Logo = "https://static2.finnhub.io/file/publicdatany/finnhubimage/stock_logo/AAPL.png"
            };
            var expected = new StockInformations
            {
                StockSymbol = companyProfile.Ticker,
                StockName = companyProfile.Name,
                Currency = companyProfile.Currency,
                Exchange = companyProfile.Exchange,
                WebUrl = companyProfile.WebUrl,
                Industry = companyProfile.FinnhubIndustry,
                PricePerShare = stockQuote.CurrentPrice,
                Logo = companyProfile.Logo
            };
            MockGetStockQuote(stockQuote);
            MockGetCompanyProfile(companyProfile);

            // Act
            var actual = await _stockService.GetStockInformations(stockSymbol);

            // Assert
            actual.Should().BeEquivalentTo(expected);
            _finnHubHttpClientMock.Verify(c => c.GetStockQuote(stockSymbol), Times.Once);
            _finnHubHttpClientMock.Verify(c => c.GetCompanyProfile(stockSymbol), Times.Once);
        }

        [Fact]
        public async Task GetStockInformations_NullStockQuote_ThrowsStockNotFoundException()
        {
            // Arrange
            const string stockSymbol = "INVALID";
            MockGetStockQuote(null);
            MockGetCompanyProfile(new CompanyProfileDTO { Ticker = stockSymbol, Name = "Invalid Stock" });

            // Act
            Func<Task> act = () => _stockService.GetStockInformations(stockSymbol);

            // Assert
            await act.Should().ThrowAsync<StockNotFoundException>()
                .WithMessage($"Stock with symbol '{stockSymbol}' was not found.");
        }

        [Fact]
        public async Task GetStockInformations_ZeroCurrentPrice_ThrowsStockNotFoundException()
        {
            // Arrange
            const string stockSymbol = "AAPL";
            MockGetStockQuote(new StockQuoteDTO { CurrentPrice = 0 });
            MockGetCompanyProfile(new CompanyProfileDTO { Ticker = stockSymbol, Name = "Apple Inc" });

            // Act
            Func<Task> act = () => _stockService.GetStockInformations(stockSymbol);

            // Assert
            await act.Should().ThrowAsync<StockNotFoundException>();
        }

        [Fact]
        public async Task GetStockInformations_NullCompanyProfile_ThrowsStockNotFoundException()
        {
            // Arrange
            const string stockSymbol = "INVALID";
            MockGetStockQuote(new StockQuoteDTO { CurrentPrice = 192.53m });
            MockGetCompanyProfile(null);

            // Act
            Func<Task> act = () => _stockService.GetStockInformations(stockSymbol);

            // Assert
            await act.Should().ThrowAsync<StockNotFoundException>();
        }

        #endregion

        private SellOrder CreateSellOrder() =>
            SellOrder.Create(TestUserId, "AAPL", "Apple Inc", 100, 5, DateTime.UtcNow, _matchingOptions.MatchBucketSize);
    }
}