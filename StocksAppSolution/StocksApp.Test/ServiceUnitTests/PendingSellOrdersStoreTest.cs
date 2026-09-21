using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using StocksApp.Domain.Events;
using StocksApp.OrdersWorker.Stores;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class PendingSellOrdersStoreTest
    {
        private readonly PendingSellOrdersStore _sellOrdersStore;

        public PendingSellOrdersStoreTest()
        {
            _sellOrdersStore = new PendingSellOrdersStore(NullLogger<PendingSellOrdersStore>.Instance);
        }

        #region TakeTriggeredOrders
        [Fact]
        public void TakeTriggeredOrders_UnknownSymbol_ReturnsEmptyList()
        {
            // Arrange
            var order = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();
            _sellOrdersStore.AddSellOrder(order);

            // Act
            var actual = _sellOrdersStore.TakeTriggeredOrders("MSFT", 1000);

            // Assert
            actual.Should().BeEmpty();
        }

        [Theory]
        [InlineData(99.99, 0)]
        [InlineData(100, 1)]
        [InlineData(100.01, 1)]
        public void TakeTriggeredOrders_PriceComparedToTarget_TriggersOnlyWhenPriceReachesTarget(
            double currentPrice, int expectedCount)
        {
            // Arrange
            var order = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();
            _sellOrdersStore.AddSellOrder(order);

            // Act
            var actual = _sellOrdersStore.TakeTriggeredOrders("AAPL", currentPrice);

            // Assert
            actual.Should().HaveCount(expectedCount);
        }

        [Fact]
        public void TakeTriggeredOrders_TriggeredOrders_AreRemovedFromStore()
        {
            // Arrange
            var order = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();
            _sellOrdersStore.AddSellOrder(order);

            // Act
            var first = _sellOrdersStore.TakeTriggeredOrders("AAPL", 100);
            var second = _sellOrdersStore.TakeTriggeredOrders("AAPL", 100);

            // Assert
            first.Should().ContainSingle().Which.Should().Be(order);
            second.Should().BeEmpty();
        }

        [Fact]
        public void TakeTriggeredOrders_MultipleSymbols_OnlyTakesOrdersOfRequestedSymbol()
        {
            // Arrange
            var apple = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();
            var microsoft = new SellOrderCreatedCommandBuilder().WithStockSymbol("MSFT").WithPrice(100).Build();
            _sellOrdersStore.AddSellOrder(apple);
            _sellOrdersStore.AddSellOrder(microsoft);

            // Act
            var actual = _sellOrdersStore.TakeTriggeredOrders("AAPL", 100);
            var remaining = _sellOrdersStore.TakeTriggeredOrders("MSFT", 100);

            // Assert
            actual.Should().ContainSingle().Which.Should().Be(apple);
            remaining.Should().ContainSingle().Which.Should().Be(microsoft);
        }

        [Fact]
        public void TakeTriggeredOrders_OrderReAddedAfterBeingTaken_TriggersAgain()
        {
            // Arrange: simulates the rollback path when order execution fails
            var order = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();
            _sellOrdersStore.AddSellOrder(order);
            var taken = _sellOrdersStore.TakeTriggeredOrders("AAPL", 100);

            // Act
            foreach (var takenOrder in taken)
                _sellOrdersStore.AddSellOrder(takenOrder);
            var actual = _sellOrdersStore.TakeTriggeredOrders("AAPL", 100);

            // Assert
            actual.Should().ContainSingle().Which.Should().Be(order);
        }

        [Fact]
        public void TakeTriggeredOrders_SamePrice_ReturnsOrdersByCreatedAtThenId()
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
            var actual = _sellOrdersStore.TakeTriggeredOrders("AAPL", 100);

            // Assert
            actual.Should().ContainInOrder(first, second, third);
        }
        #endregion

        #region AddSellOrder
        [Fact]
        public void AddSellOrder_SameOrderTwice_StoresItOnce()
        {
            // Arrange
            var order = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();

            // Act
            _sellOrdersStore.AddSellOrder(order);
            _sellOrdersStore.AddSellOrder(order);
            var actual = _sellOrdersStore.TakeTriggeredOrders("AAPL", 100);

            // Assert
            actual.Should().ContainSingle();
        }
        #endregion

        #region RemoveSellOrder
        [Fact]
        public void RemoveSellOrder_UnknownSymbol_DoesNotThrow()
        {
            // Arrange
            var order = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();

            // Act
            Action actual = () => _sellOrdersStore.RemoveSellOrder(order);

            // Assert
            actual.Should().NotThrow();
        }

        [Fact]
        public void RemoveSellOrder_OrderNeverAdded_LeavesExistingOrdersUntouched()
        {
            // Arrange
            var stored = new SellOrderCreatedCommandBuilder()
                .WithStockSymbol("AAPL")
                .WithPrice(100)
                .WithSellOrderId(Guid.NewGuid())
                .Build();
            var neverAdded = new SellOrderCreatedCommandBuilder()
                .WithStockSymbol("AAPL")
                .WithPrice(100)
                .WithSellOrderId(Guid.NewGuid())
                .Build();
            _sellOrdersStore.AddSellOrder(stored);

            // Act
            _sellOrdersStore.RemoveSellOrder(neverAdded);
            var actual = _sellOrdersStore.TakeTriggeredOrders("AAPL", 100);

            // Assert
            actual.Should().ContainSingle().Which.Should().Be(stored);
        }

        [Fact]
        public void RemoveSellOrder_OneOfSeveralOrders_RemovesOnlyThatOrder()
        {
            // Arrange
            var kept = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();
            var removed = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(110).Build();
            _sellOrdersStore.AddSellOrder(kept);
            _sellOrdersStore.AddSellOrder(removed);

            // Act
            _sellOrdersStore.RemoveSellOrder(removed);
            var actual = _sellOrdersStore.TakeTriggeredOrders("AAPL", 1000);

            // Assert
            actual.Should().ContainSingle().Which.Should().Be(kept);
        }
        #endregion

        #region HasPendingOrders
        [Fact]
        public void HasPendingOrders_OnlyOtherSymbolHasOrders_ReturnsFalse()
        {
            // Arrange
            var order = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();
            _sellOrdersStore.AddSellOrder(order);

            // Act
            var actual = _sellOrdersStore.HasPendingOrders("MSFT");

            // Assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void HasPendingOrders_OrderAdded_ReturnsTrue()
        {
            // Arrange
            var order = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();
            _sellOrdersStore.AddSellOrder(order);

            // Act
            var actual = _sellOrdersStore.HasPendingOrders("AAPL");

            // Assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void HasPendingOrders_AllOrdersTaken_ReturnsFalse()
        {
            // Arrange
            var order = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();
            _sellOrdersStore.AddSellOrder(order);
            _sellOrdersStore.TakeTriggeredOrders("AAPL", 100);

            // Act
            var actual = _sellOrdersStore.HasPendingOrders("AAPL");

            // Assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void HasPendingOrders_AllOrdersRemoved_ReturnsFalse()
        {
            // Arrange
            var order = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();
            _sellOrdersStore.AddSellOrder(order);
            _sellOrdersStore.RemoveSellOrder(order);

            // Act
            var actual = _sellOrdersStore.HasPendingOrders("AAPL");

            // Assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void HasPendingOrders_SomeOrdersStillBelowTarget_ReturnsTrueAfterPartialTake()
        {
            // Arrange
            var triggered = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build();
            var stillPending = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(200).Build();
            _sellOrdersStore.AddSellOrder(triggered);
            _sellOrdersStore.AddSellOrder(stillPending);
            _sellOrdersStore.TakeTriggeredOrders("AAPL", 100);

            // Act
            var actual = _sellOrdersStore.HasPendingOrders("AAPL");

            // Assert
            actual.Should().BeTrue();
        }
        #endregion

        #region Concurrency
        [Fact]
        public async Task ConcurrentAddAndTake_NeverLosesOrDuplicatesOrders()
        {
            // Arrange
            const int orderCount = 500;
            var orders = Enumerable.Range(1, orderCount)
                .Select(price => new SellOrderCreatedCommandBuilder()
                    .WithStockSymbol("AAPL")
                    .WithPrice(price)
                    .Build())
                .ToList();
            var taken = new ConcurrentBag<SellOrderCreatedCommand>();

            // Act
            var producer = Task.Run(() => Parallel.ForEach(orders, _sellOrdersStore.AddSellOrder));

            var consumers = Enumerable.Range(0, 4)
                .Select(_ => Task.Factory.StartNew(() =>
                {
                    while (!producer.IsCompleted)
                    {
                        foreach (var order in _sellOrdersStore.TakeTriggeredOrders("AAPL", double.MaxValue))
                            taken.Add(order);
                    }
                }, TaskCreationOptions.LongRunning))
                .ToArray();

            await Task.WhenAll(consumers.Append(producer));

            foreach (var order in _sellOrdersStore.TakeTriggeredOrders("AAPL", double.MaxValue))
                taken.Add(order); // drain anything added after the last take

            // Assert
            taken.Should().HaveCount(orderCount);
            taken.Should().OnlyHaveUniqueItems();
            _sellOrdersStore.HasPendingOrders("AAPL").Should().BeFalse();
        }
        #endregion

    }
}
