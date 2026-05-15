using Moq;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using Xunit;
using FluentAssertions;

namespace StocksApp.Test.ServiceUnitTests
{
    public class OrdersServiceTest
    {
        private readonly IOrdersService _ordersService;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<ISubscriptionsManager> _subscriptionsManagerMock;

        public OrdersServiceTest()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _subscriptionsManagerMock = new Mock<ISubscriptionsManager>();
            _ordersService = new OrdersService(_subscriptionsManagerMock.Object, _orderRepositoryMock.Object);
        }
        [Fact]
        public async Task AddSellOrder_ShouldAddSellOrderAndSubscribeToStockSymbol()
        {
            // Arrange
            var sellOrderAddRequest = new SellOrderAddRequest
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 150,
                DateAndTimeOfOrder = Convert.ToDateTime("2000-01-01"),
                StockName = "Apple Inc."
            };
            var expectedSellOrder = sellOrderAddRequest.ToSellOrder();
            _orderRepositoryMock.Setup(repo => repo.AddSellOrderAsync(It.IsAny<SellOrder>()))
                .ReturnsAsync(expectedSellOrder);
            // Act
            SellOrderResponse result = await _ordersService.AddSellOrder(sellOrderAddRequest);
            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSellOrder.ToSellOrderResponse());

            _subscriptionsManagerMock.Verify(manager => manager.AddStockSymbol(sellOrderAddRequest.StockSymbol!), Times.Once);
        }

        [Fact]
        public async Task AddSellOrder_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Func<Task> action = async () => await _ordersService.AddSellOrder(null!);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }
    }
}
