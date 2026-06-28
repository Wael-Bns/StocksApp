using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using StocksApp.Domain.Events;
using StocksApp.OrdersWorker.Stores;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class SellOrdersStoreTest
    {
        private readonly SellOrdersStore _sellOrdersStore;

        public SellOrdersStoreTest()
        {
            _sellOrdersStore = new SellOrdersStore(NullLogger<SellOrdersStore>.Instance);
        }

        [Fact]
        public void DequeueEligibleOrders_PriceBelowTarget_ReturnsEmptyList()
        {
            // Arrange
            var order = CreateSellOrder(price: 150);
            _sellOrdersStore.AddSellOrder(order);

            // Act
            var actual = _sellOrdersStore.DequeueEligibleOrders("AAPL", 149);

            // Assert
            actual.Should().BeEmpty();
        }

        [Fact]
        public void DequeueEligibleOrders_PriceReachesTargets_ReturnsOnlyEligibleOrders()
        {
            // Arrange
            var firstEligible = CreateSellOrder(price: 100);
            var secondEligible = CreateSellOrder(price: 120);
            var notEligible = CreateSellOrder(price: 130);

            _sellOrdersStore.AddSellOrder(notEligible);
            _sellOrdersStore.AddSellOrder(secondEligible);
            _sellOrdersStore.AddSellOrder(firstEligible);

            // Act
            var actual = _sellOrdersStore.DequeueEligibleOrders("AAPL", 120);
            var remaining = _sellOrdersStore.DequeueEligibleOrders("AAPL", 1000);

            // Assert
            actual.Should().ContainInOrder(firstEligible, secondEligible);
            remaining.Should().ContainSingle().Which.Should().Be(notEligible);
        }

        [Fact]
        public void DequeueEligibleOrders_SamePrice_ReturnsOrdersByCreatedAtThenId()
        {
            // Arrange
            var first = CreateSellOrder(price: 100, createdAt: new DateTime(2026, 1, 1), id: Guid.Parse("11111111-1111-1111-1111-111111111111"));
            var second = CreateSellOrder(price: 100, createdAt: new DateTime(2026, 1, 2), id: Guid.Parse("22222222-2222-2222-2222-222222222222"));
            var third = CreateSellOrder(price: 100, createdAt: new DateTime(2026, 1, 2), id: Guid.Parse("33333333-3333-3333-3333-333333333333"));

            _sellOrdersStore.AddSellOrder(third);
            _sellOrdersStore.AddSellOrder(second);
            _sellOrdersStore.AddSellOrder(first);

            // Act
            var actual = _sellOrdersStore.DequeueEligibleOrders("AAPL", 100);

            // Assert
            actual.Should().ContainInOrder(first, second, third);
        }

        [Fact]
        public void RemoveSellOrder_ExistingOrder_RemovesOrderFromStore()
        {
            // Arrange
            var order = CreateSellOrder(price: 100);
            _sellOrdersStore.AddSellOrder(order);

            // Act
            _sellOrdersStore.RemoveSellOrder(order);
            var actual = _sellOrdersStore.DequeueEligibleOrders("AAPL", 100);

            // Assert
            actual.Should().BeEmpty();
        }

        private static SellOrderCreatedCommand CreateSellOrder(
            double price,
            DateTime? createdAt = null,
            Guid? id = null)
        {
            return new SellOrderCreatedCommand
            {
                SellOrderId = id ?? Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StockSymbol = "AAPL",
                Price = price,
                Quantity = 10,
                CreatedAt = createdAt ?? DateTime.UtcNow
            };
        }
    }
}
