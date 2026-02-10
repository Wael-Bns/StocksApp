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
        // A sample BuyOrderRequest object that can be used in multiple tests
        private BuyOrderRequest buyOrderRequest = new BuyOrderRequest
        {
            StockName = "APPLE INC",
            StockSymbol = "AAPL",
            Quantity = 250,
            DateAndTimeOfOrder = DateTime.Now,
            Price = 100,
        };
        private SellOrderRequest sellOrderRequest = new SellOrderRequest
        {
            StockName = "APPLE INC",
            StockSymbol = "AAPL",
            Quantity = 250,
            DateAndTimeOfOrder = DateTime.Now,
            Price = 100,
        };

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
            buyOrderRequest.Quantity = 0;
            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(buyOrderRequest.ToBuyOrder());

            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(buyOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateBuyOrder_OrderQuantityMoreThanMaximum()
        {
            // Arrange 
            buyOrderRequest.Quantity = 10001;

            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(buyOrderRequest.ToBuyOrder());

            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(buyOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateBuyOrder_PriceLessThanMinimum()
        {
            // Arrange 
            buyOrderRequest.Price = 0;
            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(buyOrderRequest.ToBuyOrder());

            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(buyOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateBuyOrder_PriceMoreThanMaximum()
        {
            // Arrange 
            buyOrderRequest.Price = 10001;

            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(buyOrderRequest.ToBuyOrder());

            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(buyOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateBuyOrder_NullStockSymbol()
        {
            // Arrange 
            buyOrderRequest.StockSymbol = null;

            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(buyOrderRequest.ToBuyOrder());
            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(buyOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateBuyOrder_DateLessThanMinimum()
        {
            // Arrange 
            buyOrderRequest.DateAndTimeOfOrder = Convert.ToDateTime("1999-12-31");

            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(buyOrderRequest.ToBuyOrder());
            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(buyOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateBuyOrder_ValidRequest()
        {
            // Arrange
            BuyOrder buyOrder = buyOrderRequest.ToBuyOrder();
            BuyOrderResponse expected = buyOrder.ToBuyOrderResponse();

            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(buyOrder);
            // Act
            BuyOrderResponse actual = await _stockService.CreateBuyOrder(buyOrderRequest);
            // Assert
            actual.Should().BeEquivalentTo(expected);
            actual.BuyOrderID.Should().NotBeEmpty();
        }
        #endregion

        #region CreateSellOrder
        [Fact]
        public async Task CreateSellOrder_NullRequest()
        {
            //Arrange 
            SellOrderRequest? orderRequest = null;
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(orderRequest);
            };
            await actual.Should().ThrowAsync<ArgumentNullException>();
        }
        [Fact]
        public async Task CreateSellOrder_OrderQuantityLessThanMinimum()
        {
            // Arrange 
            sellOrderRequest.Quantity = 0;

            _orderRepositoryMock.Setup(repo => repo.AddSellOrderAsync(It.IsAny<SellOrder>()))
                .ReturnsAsync(sellOrderRequest.ToSellOrder());

            // Act
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(sellOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateSellOrder_OrderQuantityMoreThanMaximum()
        {
            // Arrange 
            sellOrderRequest.Quantity = 10001;

            _orderRepositoryMock.Setup(repo => repo.AddSellOrderAsync(It.IsAny<SellOrder>()))
                .ReturnsAsync(sellOrderRequest.ToSellOrder());

            // Act
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(sellOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateSellOrder_PriceLessThanMinimum()
        {
            // Arrange 
            sellOrderRequest.Price = 0;

            _orderRepositoryMock.Setup(repo => repo.AddSellOrderAsync(It.IsAny<SellOrder>()))
                .ReturnsAsync(sellOrderRequest.ToSellOrder());

            // Act
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(sellOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateSellOrder_PriceMoreThanMaximum()
        {
            // Arrange 
            sellOrderRequest.Price = 10001;

            _orderRepositoryMock.Setup(repo => repo.AddSellOrderAsync(It.IsAny<SellOrder>()))
                .ReturnsAsync(sellOrderRequest.ToSellOrder());

            // Act
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(sellOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateSellOrder_NullStockSymbol()
        {
            // Arrange 
            sellOrderRequest.StockSymbol = null;

            _orderRepositoryMock.Setup(repo => repo.AddSellOrderAsync(It.IsAny<SellOrder>()))
                .ReturnsAsync(sellOrderRequest.ToSellOrder());
            // Act
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(sellOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateSellOrder_DateLessThanMinimum()
        {
            // Arrange 
            sellOrderRequest.DateAndTimeOfOrder = Convert.ToDateTime("1999-12-31");

            _orderRepositoryMock.Setup(repo => repo.AddSellOrderAsync(It.IsAny<SellOrder>()))
                .ReturnsAsync(sellOrderRequest.ToSellOrder());
            // Act
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(sellOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task CreateSellOrder_ValidRequest()
        {
            // Arrange
            SellOrder sellOrder = sellOrderRequest.ToSellOrder();
            SellOrderResponse expected = sellOrder.ToSellOrderResponse();

            _orderRepositoryMock.Setup(repo => repo.AddSellOrderAsync(It.IsAny<SellOrder>()))
                .ReturnsAsync(sellOrder);
            // Act
            SellOrderResponse actual = await _stockService.CreateSellOrder(sellOrderRequest);
            // Assert
            actual.Should().BeEquivalentTo(expected);
            actual.SellOrderID.Should().NotBeEmpty();
        }
        #endregion

        #region GetAllBuyOrders
        [Fact]
        public async Task GetAllBuyOrders_Empty()
        {
            // Arrange
            _orderRepositoryMock.Setup(repo => repo.GetAllBuyOrdersAsync())
                .ReturnsAsync(new List<BuyOrder>());
            // Act
            List<BuyOrderResponse> actual = await _stockService.GetAllBuyOrders();
            // Assert
            actual.Should().BeEmpty();
        }
        [Fact]
        public async Task GetAllBuyOrders_Successful()
        {
            // Arrange 
            List<BuyOrder> buyOrders = [];
            buyOrders.Add(buyOrderRequest.ToBuyOrder());
            
            _orderRepositoryMock.Setup(repo => repo.GetAllBuyOrdersAsync())
                .ReturnsAsync(buyOrders);
            
            List<BuyOrderResponse> expected = buyOrders.Select(order => order.ToBuyOrderResponse()).ToList();
            
            // Act
            List<BuyOrderResponse> actual = await _stockService.GetAllBuyOrders();
            
            // Assert
            actual.Should().BeEquivalentTo(expected);

        }
        #endregion

        #region GetAllSellOrders
        [Fact]
        public async Task GetAllSellOrders_Empty()
        {
            // Arrange
            _orderRepositoryMock.Setup(repo => repo.GetAllSellOrdersAsync())
                .ReturnsAsync(new List<SellOrder>());
            // Act
            List<SellOrderResponse> actual = await _stockService.GetAllSellOrders();
            // Assert
            actual.Should().BeEmpty();
        }
        [Fact]
        public async Task GetAllSellOrders_Successful()
        {
            // Arrange 
            List<SellOrder> sellOrders = [];
            sellOrders.Add(sellOrderRequest.ToSellOrder());

            _orderRepositoryMock.Setup(repo => repo.GetAllSellOrdersAsync())
                .ReturnsAsync(sellOrders);

            List<SellOrderResponse> expected = sellOrders.Select(order => order.ToSellOrderResponse()).ToList();

            // Act
            List<SellOrderResponse> actual = await _stockService.GetAllSellOrders();

            // Assert
            actual.Should().BeEquivalentTo(expected);

        }
        #endregion

    }
}
