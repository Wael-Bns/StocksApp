using AutoFixture;
using FluentAssertions;
using Moq;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.DTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using Xunit;

namespace StocksApp.Test.ServiceTests
{
    public class StockServiceTest
    {
        private readonly IStockService _stockService;
        private readonly IFixture _fixture;
        private readonly IOrderRepository _orderRepository;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;

        public StockServiceTest()
        {
            // Mock the IOrderRepository
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _orderRepository = _orderRepositoryMock.Object;
            // Pass the mocked repository to the Service
            _stockService = new StockService(_orderRepository);
            // Use AutoFixture to create test data
            _fixture = new Fixture();
        }

        #region CreateBuyOrder
        [Fact]
        public async Task CreateBuyOrder_NullRequest()
        {
            //Arrange 
            BuyOrderRequest? orderRequest = null;
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(orderRequest);
            };
            await actual.Should().ThrowAsync<ArgumentNullException>();
        }
        [Fact]
        public async Task CreateBuyOrder_OrderQuantityLessThanMinimum()
        {
            // Arrange 
            BuyOrderRequest orderRequest = _fixture.Build<BuyOrderRequest>()
                .With(o => o.Quantity, Convert.ToUInt32(0))
                .Create();
            
            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(orderRequest.ToBuyOrder());

            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(orderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateBuyOrder_OrderQuantityMoreThanMaximum()
        {
            // Arrange 
            BuyOrderRequest orderRequest = _fixture.Build<BuyOrderRequest>()
                .With(o => o.Quantity, Convert.ToUInt32(10001))
                .Create();

            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(orderRequest.ToBuyOrder());

            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(orderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateBuyOrder_PriceLessThanMinimum()
        {
            // Arrange 
            BuyOrderRequest orderRequest = _fixture.Build<BuyOrderRequest>()
                .With(o => o.Price, 0)
                .Create();

            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(orderRequest.ToBuyOrder());

            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(orderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateBuyOrder_PriceMoreThanMaximum()
        {
            // Arrange 
            BuyOrderRequest orderRequest = _fixture.Build<BuyOrderRequest>()
                .With(o => o.Price, 10001)
                .Create();

            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(orderRequest.ToBuyOrder());

            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(orderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateBuyOrder_NullStockSymbol()
        {
                       // Arrange 
            BuyOrderRequest orderRequest = _fixture.Build<BuyOrderRequest>()
                .With(o => o.StockSymbol, null as string)
                .Create();
            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(orderRequest.ToBuyOrder());
            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(orderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateBuyOrder_DateLessThanMinimum()
        {
            // Arrange 
            BuyOrderRequest orderRequest = _fixture.Build<BuyOrderRequest>()
                .With(o => o.DateAndTimeOfOrder, Convert.ToDateTime("1999-12-31"))
                .Create();
            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(orderRequest.ToBuyOrder());
            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(orderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateBuyOrder_ValidRequest()
        {
            // Arrange
            BuyOrderRequest orderRequest = new BuyOrderRequest
            {
                StockSymbol = "AAPL",
                StockName = "Apple Inc.",
                DateAndTimeOfOrder = DateTime.Now,
                Quantity = 100,
                Price = 150.00
            };

            BuyOrder buyOrder = orderRequest.ToBuyOrder();
            BuyOrderResponse expected = buyOrder.ToBuyOrderResponse();

            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(buyOrder);
            // Act
            BuyOrderResponse actual = await _stockService.CreateBuyOrder(orderRequest);
            // Assert
            actual.Should().BeEquivalentTo(expected);
            actual.BuyOrderID.Should().NotBeEmpty();
        }
        #endregion
    }
}
