using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using StocksApp.OrdersWorker.Stores;
using StocksApp.Tests.Common.Builders;
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
            var order = new SellOrderCreatedCommandBuilder()
                .WithStockSymbol("AAPL")
                .WithPrice(150)
                .Build();
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
            var firstEligible = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();
            var secondEligible = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(120).Build();
            var notEligible = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(130).Build();

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
            var first = new SellOrderCreatedCommandBuilder()
                .WithStockSymbol("AAPL")
                .WithPrice(100)
                .WithCreatedAt(new DateTime(2026, 1, 1))
                .WithSellOrderId(Guid.Parse("11111111-1111-1111-1111-111111111111"))
                .Build();
            var second = new SellOrderCreatedCommandBuilder()
                .WithStockSymbol("AAPL")
                .WithPrice(100)
                .WithCreatedAt(new DateTime(2026, 1, 2))
                .WithSellOrderId(Guid.Parse("22222222-2222-2222-2222-222222222222"))
                .Build();
            var third = new SellOrderCreatedCommandBuilder()
                .WithStockSymbol("AAPL")
                .WithPrice(100)
                .WithCreatedAt(new DateTime(2026, 1, 2))
                .WithSellOrderId(Guid.Parse("33333333-3333-3333-3333-333333333333"))
                .Build();

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
            var order = new SellOrderCreatedCommandBuilder()
                .WithStockSymbol("AAPL")
                .WithPrice(100)
                .Build();
            _sellOrdersStore.AddSellOrder(order);

            // Act
            _sellOrdersStore.RemoveSellOrder(order);
            var actual = _sellOrdersStore.DequeueEligibleOrders("AAPL", 100);

            // Assert
            actual.Should().BeEmpty();
        }
    }
}
