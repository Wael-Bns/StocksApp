using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.Enums;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class OrdersExecutorTest
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly IOrdersExecutor _ordersExecutor;

        public OrdersExecutorTest()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            var loggerMock = new Mock<ILogger<OrdersExecutor>>();
            _ordersExecutor = new OrdersExecutor(_orderRepositoryMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteSellOrders_WhenRepositoryReturnsNull_ShouldReturnNull()
        {
            // Arrange
            const string stockSymbol = "AAPL";
            const double marketPrice = 150;

            _orderRepositoryMock
                .Setup(repo => repo.ExecuteSellOrders(stockSymbol, marketPrice))
                .ReturnsAsync((IEnumerable<SellOrder>?)null);

            // Act
            List<SellOrderResponse>? result = await _ordersExecutor.ExecuteSellOrders(stockSymbol, marketPrice);

            // Assert
            result.Should().BeNull();
            _orderRepositoryMock.Verify(repo => repo.ExecuteSellOrders(stockSymbol, marketPrice), Times.Once);
        }

        [Fact]
        public async Task ExecuteSellOrders_WhenRepositoryReturnsExecutedOrders_ShouldReturnResponses()
        {
            // Arrange
            const string stockSymbol = "AAPL";
            const double marketPrice = 150;

            List<SellOrder> executedOrders =
            [
                new SellOrder
                {
                    SellOrderID = Guid.NewGuid(),
                    StockSymbol = stockSymbol,
                    StockName = "Apple Inc.",
                    DateAndTimeOfOrder = DateTime.UtcNow,
                    Price = 140,
                    Quantity = 10,
                    Status = (int)SellOrderStatus.Executed,
                    UserId = Guid.NewGuid(),
                    User = new User()
                }
            ];

            _orderRepositoryMock
                .Setup(repo => repo.ExecuteSellOrders(stockSymbol, marketPrice))
                .ReturnsAsync(executedOrders);

            List<SellOrderResponse> expected = executedOrders
                .Select(order => order.ToSellOrderResponse())
                .ToList();

            // Act
            List<SellOrderResponse>? result = await _ordersExecutor.ExecuteSellOrders(stockSymbol, marketPrice);

            // Assert
            result.Should().BeEquivalentTo(expected);
            _orderRepositoryMock.Verify(repo => repo.ExecuteSellOrders(stockSymbol, marketPrice), Times.Once);
        }
    }
}
